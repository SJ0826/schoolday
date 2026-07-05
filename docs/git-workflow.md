# Git 전략 — schoolday

> 1인(소규모) Unity 게임 개발 기준. **최대한 단순하게, 귀찮은 건 자동화한다.**
> 원칙: 브랜치·이슈·PR을 손으로 관리하지 않는다. `/ship` 하나로 끝낸다.

## 브랜치 모델

- **`main`** — 항상 "플레이 가능한" 안정 버전. **직접 커밋하지 않는다.**
- **작업 브랜치** — Conductor 워크스페이스가 자동으로 만들어 주는 브랜치에서 작업한다.
  직접 만들 때는 아래 규칙으로 이름 짓는다.

| 접두사 | 용도 | 예시 |
|---|---|---|
| `feat/` | 새 기능·시스템 | `feat/first-person-controller` |
| `fix/` | 버그 수정 | `fix/door-interaction` |
| `chore/` | 설정·문서·리팩터 | `chore/gitignore` |

## 일하는 흐름 (이게 전부다)

1. 작업 브랜치에서 코드/씬을 수정한다.
2. 끝나면 **`/ship`** 을 실행한다 → 커밋 메시지 자동 작성 → 커밋 → 푸시 → `main` 대상 PR 생성/머지까지 자동.
3. 중간 저장이 필요하면 **`/save`** → 로컬 체크포인트 커밋(푸시·PR 없음).

> 이슈는 원칙적으로 만들지 않는다. 할 일은 `docs/roadmap.md` 체크리스트로 관리한다.
> 정말 추적이 필요한 큰 건만 `/ship` 이 GitHub 이슈로 승격한다.

## 커밋 컨벤션

- 형식: `<타입>: <한글 요약>` (예: `feat: 1인칭 이동 + 마우스 시점`)
- 타입: `feat` / `fix` / `chore` / `docs` / `refactor` / `art`(에셋)
- **`Co-Authored-By:` 줄은 절대 넣지 않는다.**

## Unity 특화 규칙

- `Library/`, `Logs/`, `UserSettings/`, `Temp/`, `Build/` 는 **절대 커밋하지 않는다** (`.gitignore` 처리됨).
- 씬(`.unity`)·프리팹(`.prefab`)은 병합 충돌이 잘 나므로 **가능하면 한 브랜치에서 한 사람만** 만진다.
  충돌 시 Unity Smart Merge(`.gitattributes`의 `unityyamlmerge`)를 쓴다.
- 대용량 바이너리(이미지·모델·사운드)는 Git LFS로 관리한다. 최초 1회 `git lfs install` 필요.
- `.meta` 파일은 **반드시 짝이 되는 에셋과 함께 커밋**한다(누락 시 Unity가 깨진다).

## 로드맵 = 할 일 목록

세부 단계는 `docs/roadmap.md` 참고. 기획 전문은 `docs/game-design.md`.
