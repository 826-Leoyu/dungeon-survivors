# CI: Unity tests on GitHub Actions

`.github/workflows/unity-tests.yml` runs the Unity EditMode tests with [GameCI](https://game.ci) on every pull request, on every push to `main`, and on demand (Actions tab → **Unity tests** → **Run workflow**).
The result shows at the bottom of a PR as the **Test Results** check. The test result XML is attached to each run as the `test-results` artifact.

- Unity version: read from `game/ProjectSettings/ProjectVersion.txt`. When you upgrade Unity, CI follows automatically, as long as GameCI has published an image for that version (`unityci/editor:ubuntu-<version>-base-3`).
- `game/Library` is cached between runs, so the first run is slow (full import, about 5 min) and later runs are faster (about 3 min).
  A cache saved by a PR run is visible only to that PR; the cache saved by a run on `main` is visible to every branch, so each merge to `main` warms the cache for later PRs.
- Only EditMode tests run. There are no PlayMode tests yet; add `testMode: All` when there are.

## Secrets (Unity Personal license)

Unity must be activated to run, also on GitHub's servers. The workflow reads three repository secrets (GitHub → **Settings** → **Secrets and variables** → **Actions**):

| Secret | Value | Used for |
|---|---|---|
| `UNITY_EMAIL` | The email of the Unity account | Activation (what actually works) |
| `UNITY_PASSWORD` | The password of the Unity account | Activation (what actually works) |
| `UNITY_LICENSE` | The full contents of the license file `C:\ProgramData\Unity\Unity_lic.ulf` | Tried first, then skipped (see below) |

In practice CI activates with `UNITY_EMAIL` / `UNITY_PASSWORD`. The `.ulf` file is bound to the machine that created it, so on GitHub's runners the test runner logs
`The license file's machine binding doesn't match this machine - falling back to activating with the Unity account (UNITY_EMAIL/UNITY_PASSWORD) instead.`
and signs in with the account. That warning is expected. `UNITY_LICENSE` is kept because GameCI's documented Personal setup expects it.

To get `Unity_lic.ulf` (only needed if you ever set the secret again): Unity Hub can show an active Personal license without having written the file. Sign out of Unity Hub, quit it, sign back in, then **Manage licenses** → **Add** → **Get a free personal license**. The file then appears at the path above.
To copy it without opening it, run in WSL: `cat /mnt/c/ProgramData/Unity/Unity_lic.ulf | clip.exe`.

## When CI fails with a license error

- **Sign-in failed:** the Unity password changed, or the account now has two-factor authentication (CI cannot enter a code). Update `UNITY_PASSWORD` (**Update secret**), or turn off 2FA for this account, then re-run the failed job.
- **No seats available:** see GameCI's [returning a license](https://game.ci/docs/github/returning-a-license).
- **Looks like a license problem but keeps retrying:** Unity 6.6+ needs extra shared memory; `game-ci/unity-test-runner` v4.3.2 and later handle it. Check that `@v4` still resolves to such a version.
