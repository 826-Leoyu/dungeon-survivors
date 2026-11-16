## 需要本人动手的步骤

凡是需要本人操作的事（在 Unity 编辑器里点按、跑 Test Runner、推送、配置 GitHub / itch.io 等），回复末尾都要给出「你要做的」清单：编号步骤，每步写明点哪里 / 输入什么、预期看到什么，以及做完后回报什么（成功怎么说、失败把什么发回来）。专业名词第一次出现时用一句话解释它是什么、为什么要做。

## Running tests

- Preferred: Unity MCP (run EditMode tests, read the console). It only connects when a Claude session starts, so the user must have the Editor open and have clicked **Window → MCP for Unity → Start Server** *before* starting Claude. If it fails to connect mid-session, ask the user to do that and restart the session, or use the fallback below.
- Fallback when Unity MCP is down (WSL, Editor open or closed):
  - `tools/wsl-tests/run-rules-tests.sh [TestClass]` compiles `Scripts/Rules` + `Tests/EditMode` with Unity's bundled Roslyn and runs them. It supports `[Test]`/`[TestCase]` only and skips the Pooling tests, so the Unity Test Runner is still the final check before a PR.
  - `tools/wsl-tests/typecheck-unity.sh` typechecks `Scripts/**` and `Editor/**` against Unity's DLLs (CS0649 warnings are expected).
  - Output goes to `/tmp/dungeon-survivors-tests`, never inside the repo. Set `UNITY_DATA_WIN` if Unity isn't at `D:\Development\Unity\6000.6.8f1`.

## Agent skills

### Issue tracker

Issues and specs live in GitHub Issues for 826-Leoyu/dungeon-survivors (via `gh`). See `docs/agents/issue-tracker.md`.

### Triage labels

Default five-role vocabulary: needs-triage, needs-info, ready-for-agent, ready-for-human, wontfix. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one `GLOSSARY.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
