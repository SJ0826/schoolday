using UnityEngine;
using UnityEditor;

/// <summary>
/// Render Pipeline Converter가 놓친 머티리얼(핑크로 보이는 것)을 URP/Lit 으로 강제 변환한다.
/// 기존 메인 텍스처(_MainTex)와 색(_Color)은 URP 프로퍼티(_BaseMap/_BaseColor)로 옮겨 보존.
/// 메뉴 [SchoolDay ▸ School 머티리얼 URP로 고치기].
/// </summary>
public static class FixSchoolMaterials
{
    [MenuItem("SchoolDay/School 머티리얼 URP로 고치기")]
    static void Fix()
    {
        var urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("[SchoolDay] URP/Lit 셰이더를 찾을 수 없습니다.");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/school" });
        int fixedCount = 0;

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            if (mat.shader == urpLit) continue;   // 이미 정상

            // 기존 텍스처/색 최대한 보존
            Texture tex = null;
            if (mat.HasProperty("_MainTex")) tex = mat.GetTexture("_MainTex");
            else if (mat.HasProperty("_BaseMap")) tex = mat.GetTexture("_BaseMap");

            Color col = Color.white;
            if (mat.HasProperty("_Color")) col = mat.GetColor("_Color");
            else if (mat.HasProperty("_BaseColor")) col = mat.GetColor("_BaseColor");

            mat.shader = urpLit;
            if (tex != null) mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", col);

            EditorUtility.SetDirty(mat);
            fixedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SchoolDay] 머티리얼 {fixedCount}개를 URP/Lit 으로 변환했습니다. (전체 {guids.Length}개 중)");
    }
}
