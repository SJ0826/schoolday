# schoolday — 프로젝트 지침

3D 1인칭 심리 공포게임 《방과 후》. **Unity 6000.5.2f1 / C#**.
기획 전문: [docs/game-design.md](docs/game-design.md) · 로드맵: [docs/roadmap.md](docs/roadmap.md) · Git 전략: [docs/git-workflow.md](docs/git-workflow.md)

## Git — 매번 묻지 말고 알아서 처리한다

사용자는 브랜치/이슈/PR 관리를 귀찮아한다. 아래 규칙을 **기본값**으로 삼고 일일이 확인받지 않는다.

- **`main`에 직접 커밋 금지.** 항상 작업 브랜치(`feat/` `fix/` `chore/`)에서 작업한다.
- 작업이 일단락되면 알아서 `/ship` 흐름으로 커밋→푸시→PR→머지까지 진행한다.
  (중간 저장만 필요하면 `/save`.)
- 커밋 메시지: `<타입>: <한글 요약>` (feat/fix/chore/docs/refactor/art).
- **`Co-Authored-By:` 줄은 절대 넣지 않는다.**
- 완료한 로드맵 항목은 `docs/roadmap.md`에서 `[x]` 체크한다.

## Unity 규칙 (중요)

- `Library/` `Logs/` `Temp/` `Build/` `UserSettings/` 는 **절대 커밋하지 않는다** (`.gitignore` 적용됨).
  git이 추적하는 것은 `Assets/`, `Packages/`, `ProjectSettings/`, `docs/`, 설정 파일뿐이다.
- 새 에셋은 **항상 짝이 되는 `.meta` 파일과 함께** 커밋한다 (누락 시 Unity 참조가 깨짐).
- 씬(`.unity`)·프리팹(`.prefab`)은 병합 충돌이 잦다. 가능하면 한 브랜치에서만 수정한다.
- 대용량 바이너리(png/fbx/wav 등)는 Git LFS 대상이다 (`.gitattributes`). 최초 1회 `git lfs install`.

## 개발 원칙 (기획서 발췌)

- 지금 목표는 완성이 아니라 **수직 슬라이스**(조례+1교시, 5분)다.
- 처음엔 **그레이박스**(회색 상자)로 로직부터 완성하고, 에셋은 나중에 교체한다.
- 공포의 핵심은 괴물이 아니라 **위화감**과 **되돌릴 수 없다는 슬픔**이다.
