using System.Linq;
using DungeonSurvivors.Rules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DungeonSurvivors
{
    /// <summary>
    /// The level-up choice: while the Run offers <see cref="Run.UpgradeChoices"/>, freezes the game and shows one button per option.
    /// A click, a tap or the number keys 1-3 pick an option. Several level-ups at once show their choices one after another,
    /// then the game resumes. The rules (what is offered, what a pick does) are all in the Run; this only shows them and forwards the pick.
    /// </summary>
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] private RunHost runHost;

        [Tooltip("Shown while a choice is open; hidden otherwise. Should cover the screen, so presses never reach the joystick behind it.")]
        [SerializeField] private GameObject panel;

        [Tooltip("One button per option, in order. Buttons beyond the number of options offered are hidden.")]
        [SerializeField] private Button[] optionButtons;

        [Tooltip("The label of each option button, in the same order.")]
        [SerializeField] private Text[] optionLabels;

        [Tooltip("Real-time seconds each new set of options ignores input, so a double tap does not also pick from the next set.")]
        [SerializeField, Min(0f)] private float inputDelay = 0.3f;

        private static readonly Key[] OptionKeys = { Key.Digit1, Key.Digit2, Key.Digit3 };
        private static readonly Key[] OptionNumpadKeys = { Key.Numpad1, Key.Numpad2, Key.Numpad3 };

        private bool isOpen;
        private float acceptInputAt;
        // Restored on close rather than forcing 1, so a choice never undoes a freeze someone else set (pause, end of Run).
        private float timeScaleBeforeOpen = 1f;

        private Run Run => runHost.Run;

        private void Awake()
        {
            for (var i = 0; i < optionButtons.Length; i++)
            {
                var index = i; // The lambda must capture a copy: i keeps changing.
                optionButtons[i].onClick.AddListener(() => Choose(index));
            }

            panel.SetActive(false);
        }

        // Update still runs while the time scale is 0, so the panel keeps reading input while the game is frozen.
        private void Update()
        {
            if (Run.IsOver)
            {
                // The Run ignores picks once it is over, so the choice could never close; the end-of-Run flow owns the screen now.
                if (isOpen) Hide();
                return;
            }

            if (!isOpen && Run.UpgradeChoices.Count > 0) Open();
            if (isOpen) ReadOptionKeys();
        }

        private void Open()
        {
            isOpen = true;
            timeScaleBeforeOpen = Time.timeScale;
            Time.timeScale = 0f;
            panel.SetActive(true);
            ShowChoices();
        }

        private void Close()
        {
            Hide();
            Time.timeScale = timeScaleBeforeOpen;
        }

        // Hides the panel without touching the time scale.
        private void Hide()
        {
            isOpen = false;
            panel.SetActive(false);
        }

        private void ShowChoices()
        {
            var choices = Run.UpgradeChoices;
            for (var i = 0; i < optionButtons.Length; i++)
            {
                var offered = i < choices.Count;
                optionButtons[i].gameObject.SetActive(offered);
                if (offered) optionLabels[i].text = Describe(choices[i]);
            }

            acceptInputAt = Time.unscaledTime + inputDelay;
        }

        private void ReadOptionKeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            for (var i = 0; i < OptionKeys.Length; i++)
            {
                if (keyboard[OptionKeys[i]].wasPressedThisFrame || keyboard[OptionNumpadKeys[i]].wasPressedThisFrame) Choose(i);
            }
        }

        private void Choose(int index)
        {
            var choices = Run.UpgradeChoices;
            if (!isOpen || Time.unscaledTime < acceptInputAt || index >= choices.Count) return;

            Run.ChooseUpgrade(choices[index]);
            if (Run.UpgradeChoices.Count > 0) ShowChoices();
            else Close();
        }

        // Runs only when a choice opens, so the LINQ allocation does not matter.
        private string Describe(Upgrade upgrade) =>
            OrbitingKnives.Upgrades.Contains(upgrade) ? DescribeKnives(upgrade) : DescribeStraightShot(upgrade);

        private string DescribeStraightShot(Upgrade upgrade)
        {
            var weapon = Run.StraightShot;
            var upgradeNumbers = Run.Config.StartingWeapon.Upgrades;
            var level = LevelText(weapon.LevelOf(upgrade), weapon.MaxLevelOf(upgrade));

            return upgrade switch
            {
                Upgrade.StraightShotDamage => $"Straight Shot\nDamage +{upgradeNumbers.DamagePerLevel}\n{level}",
                Upgrade.StraightShotBulletCount => $"Straight Shot\n+1 bullet\n{level}",
                Upgrade.StraightShotFireRate =>
                    $"Straight Shot\nFires {Mathf.RoundToInt((1f - upgradeNumbers.FireIntervalMultiplier) * 100f)}% sooner\n{level}",
                _ => upgrade.ToString(),
            };
        }

        private string DescribeKnives(Upgrade upgrade)
        {
            var weapon = Run.OrbitingKnives;
            var upgradeNumbers = Run.Config.OrbitingKnives.Upgrades;
            var level = LevelText(weapon.LevelOf(upgrade), weapon.MaxLevelOf(upgrade));

            return upgrade switch
            {
                Upgrade.OrbitingKnivesUnlock => $"New weapon\nOrbiting Knives\n{weapon.KnifeCount} knives circle you",
                Upgrade.OrbitingKnivesDamage => $"Orbiting Knives\nDamage +{upgradeNumbers.DamagePerLevel}\n{level}",
                Upgrade.OrbitingKnivesCount => $"Orbiting Knives\n+1 knife\n{level}",
                Upgrade.OrbitingKnivesRadius => $"Orbiting Knives\nRadius +{upgradeNumbers.RadiusPerLevel:0.##}\n{level}",
                Upgrade.OrbitingKnivesRotationSpeed =>
                    $"Orbiting Knives\nSpins {Mathf.RoundToInt(upgradeNumbers.RotationSpeedPerLevel / weapon.RotationSpeed * 100f)}% faster\n{level}",
                _ => upgrade.ToString(),
            };
        }

        // The level the option would reach if picked.
        private static string LevelText(int level, int maxLevel) => $"Lv {level + 1}/{maxLevel}";

        private void OnDestroy()
        {
            // Time scale is global and outlives this scene, so undo the freeze.
            if (isOpen) Time.timeScale = timeScaleBeforeOpen;
        }
    }
}
