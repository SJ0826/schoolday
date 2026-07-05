using UnityEngine;
using UnityEditor;

/// <summary>
/// School 에셋의 핑크(셰이더 깨짐)를 고친다.
/// 1) 별도 .mat 파일 → URP/Lit 로 변환
/// 2) FBX 안에 임베드된 머티리얼 → 현재 파이프라인(URP) 셰이더로 다시 임포트
/// 메뉴 [SchoolDay ▸ School 머티리얼 URP로 고치기].
/// </summary>
public static class FixSchoolMaterials
{
    const string Root = "Assets/school";

    [MenuItem("SchoolDay/School 머티리얼 URP로 고치기")]
    static void Fix()
    {
        FixLooseMaterials();
        ReimportModelMaterials();
        AssetDatabase.Refresh();
    }

    // 별도 .mat 파일 변환
    static void FixLooseMaterials()
    {
        var urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) { Debug.LogError("[SchoolDay] URP/Lit 셰이더 없음"); return; }

        var guids = AssetDatabase.FindAssets("t:Material", new[] { Root });
        int n = 0;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null || mat.shader == urpLit) continue;

            Texture tex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex")
                        : mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null;
            Color col = mat.HasProperty("_Color") ? mat.GetColor("_Color")
                      : mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;

            mat.shader = urpLit;
            if (tex != null) mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", col);
            EditorUtility.SetDirty(mat);
            n++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[SchoolDay] 별도 머티리얼 {n}개 URP 변환 (전체 {guids.Length})");
    }

    // FBX 임베디드 머티리얼 → URP 셰이더로 재임포트
    static void ReimportModelMaterials()
    {
        var guids = AssetDatabase.FindAssets("t:Model", new[] { Root });
        int n = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp == null) continue;
                // 현재 렌더 파이프라인(URP)에 맞는 머티리얼로 생성
                imp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                imp.SaveAndReimport();
                n++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        Debug.Log($"[SchoolDay] 모델 {n}개 URP 머티리얼로 재임포트 완료");
    }
}
