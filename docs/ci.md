# CI: Unity tests on GitHub Actions

`.github/workflows/unity-tests.yml` runs the Unity EditMode tests with [GameCI](https://game.ci) on every pull request, on every push to `main`, and on demand (Actions tab → **Unity tests** → **Run workflow**).
The result shows at the bottom of a PR as the **Test Results** check. The test result XML is attached to each run as the `test-results` artifact.

- Unity version: read from `game/ProjectSettings/ProjectVersion.txt`. When you upgrade Unity, CI follows automatically, as long as GameCI has published an image for that version (`unityci/editor:ubuntu-<version>-base-3`).
- `game/Library` is cached between runs, so the first run is slow (full import) and later runs are faster.
- Only EditMode tests run. There are no PlayMode tests yet; add `testMode: All` when there are.

## Secrets (Unity Personal license)

Unity must be activated to run, also on GitHub's servers. The workflow reads three repository secrets (GitHub → **Settings** → **Secrets and variables** → **Actions**):

| Secret | Value |
|---|---|
| `UNITY_LICENSE` | The full contents of the license file `C:\ProgramData\Unity\Unity_lic.ulf` |
| `UNITY_EMAIL` | The email of the Unity account |
| `UNITY_PASSWORD` | The password of the Unity account |

To get `Unity_lic.ulf`: sign in to Unity Hub, open **Manage licenses**, and make sure a Personal license is active (**Add** → **Get a free personal license** if none is). The file appears at the path above.

## When CI fails with a license error

The Personal license file expires or gets replaced when Unity Hub renews it. Copy the current contents of `Unity_lic.ulf` into the `UNITY_LICENSE` secret again (**Update secret**), then re-run the failed job.
If the error says no seats are available, see GameCI's [returning a license](https://game.ci/docs/github/returning-a-license).
