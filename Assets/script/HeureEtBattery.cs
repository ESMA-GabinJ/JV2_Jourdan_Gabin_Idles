using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Pour recharger la scène

public class ClockAndBatterySystem : MonoBehaviour
{
    public AudioSource musicAudioSource; // La référence à l'AudioSource de la musique
    public AudioClip night1Music; // La musique pour la nuit 1
    public AudioClip night2Music; // La musique pour la nuit 2
    public AudioClip batteryDepletedAudio; // L'audio pour la fin de la batterie

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI batteryText;
    public TextMeshProUGUI nightText; // Texte pour afficher "Nuit X"
    public TextMeshProUGUI moneyText; // Texte pour afficher l'argent
    public TextMeshProUGUI rechargePriceText; // Texte pour afficher le prix du bouton recharge
    public TextMeshProUGUI batteryDecrementPriceText; // Texte pour afficher le prix du nouvel item

    public GameObject gameOverImage; // L'image à afficher quand la batterie atteint 0%
    public GameObject secondGameOverImage; // Deuxième image à afficher lorsque la batterie atteint 0%

    private float hourCounter = 0f;
    private float timeUpdateInterval = 10f;
    private float startTime;

    private bool resetHourInProgress = false;
    private bool isWaitingAfterReset = false;

    private float battery = 100f;
    private float batteryDecrementTime = 40f; // Temps avant la décharge de la batterie
    private bool batteryResetInProgress = false;
    private float resetTimer = 0f;
    private float startBatteryTime = 0f;

    public Button toggleButton;
    public GameObject objectToDisplay;

    public float batteryIncreaseAmount = 1f;
    private int nightCounter = 1; // Compteur pour suivre la nuit actuelle
    private int money = 0; // Montant total d'argent accumulé

    private int maxAppearancesPerNight = 5; // Nombre maximum d'apparitions pour la première nuit

    // Nouveau bouton recharge
    public Button rechargeButton; // Le bouton qui va permettre d'augmenter le batteryIncreaseAmount
    public int rechargeCost = 100; // Coût de l'achat du recharge

    // Nouveau bouton pour réduire la vitesse de décharge de la batterie
    public Button batteryDecrementButton; // Le bouton qui va permettre de réduire la vitesse de décharge
    public int batteryDecrementCost = 200; // Coût initial de l'achat

    void Start()
    {
        startTime = Time.time;
        startBatteryTime = Time.time;

        if (timeText == null)
        {
            timeText = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        }

        if (batteryText == null)
        {
            batteryText = GameObject.Find("BatteryText").GetComponent<TextMeshProUGUI>();
        }

        if (nightText == null)
        {
            nightText = GameObject.Find("NightText").GetComponent<TextMeshProUGUI>();
        }

        if (moneyText == null)
        {
            moneyText = GameObject.Find("MoneyText").GetComponent<TextMeshProUGUI>();
        }

        if (rechargePriceText == null)
        {
            rechargePriceText = GameObject.Find("RechargePriceText").GetComponent<TextMeshProUGUI>();
        }

        if (batteryDecrementPriceText == null)
        {
            batteryDecrementPriceText = GameObject.Find("BatteryDecrementPriceText").GetComponent<TextMeshProUGUI>();
        }

        if (gameOverImage != null)
        {
            gameOverImage.SetActive(false); // S'assurer que l'image est cachée au début
        }

        if (secondGameOverImage != null)
        {
            secondGameOverImage.SetActive(false); // S'assurer que la deuxième image est cachée au début
        }

        if (objectToDisplay != null)
        {
            objectToDisplay.SetActive(true);
            StartCoroutine(DisplayObjectFor3Seconds());
        }

        if (toggleButton != null)
            toggleButton.onClick.AddListener(OnToggleButtonClick);

        // Initialiser l'affiche de la nuit
        DisplayNightText();
        UpdateMoneyText(); // Mise à jour initiale de l'affichage de l'argent
        UpdateRechargePriceText(); // Affiche le prix de recharge
        UpdateBatteryDecrementPriceText(); // Affiche le prix de l'item qui réduit la décharge

        // Ajoute le listener pour le bouton recharge
        if (rechargeButton != null)
        {
            rechargeButton.onClick.AddListener(OnRechargeButtonClick);
        }

        // Ajoute le listener pour le bouton de réduction de la vitesse de décharge de la batterie
        if (batteryDecrementButton != null)
        {
            batteryDecrementButton.onClick.AddListener(OnBatteryDecrementButtonClick);
        }

        // Lancer la musique de la nuit 1 par défaut
        if (musicAudioSource != null && night1Music != null)
        {
            musicAudioSource.clip = night1Music;
            musicAudioSource.Play();
        }
    }

    void Update()
    {
        if (isWaitingAfterReset)
        {
            return;
        }

        if (Time.time - startTime >= timeUpdateInterval)
        {
            if (hourCounter < 6 && !resetHourInProgress)
            {
                hourCounter++;
            }

            if (hourCounter >= 6 && !resetHourInProgress)
            {
                resetHourInProgress = true;
                StartCoroutine(ResetHourAndBattery());
            }

            startTime = Time.time;
        }

        if (batteryResetInProgress)
        {
            resetTimer += Time.deltaTime;
            if (resetTimer >= 3f)
            {
                batteryResetInProgress = false;
                resetTimer = 0f;
            }
        }
        else
        {
            if (Time.time - startBatteryTime >= 3f)
            {
                DecrementBattery();
            }
        }

        UpdateTimeText();
        UpdateBatteryText();
    }

    void UpdateTimeText()
    {
        string formattedTime = hourCounter.ToString("00") + ":00";
        if (timeText != null)
        {
            timeText.text = formattedTime;
        }
    }

    void UpdateBatteryText()
    {
        if (batteryText != null)
        {
            batteryText.text = Mathf.Round(battery).ToString() + "%";
        }

        // Si la batterie atteint 0%, afficher les deux images, redémarrer la scène après 5 secondes et jouer l'audio de fin de batterie
        if (battery <= 0f)
        {
            if (gameOverImage != null)
            {
                gameOverImage.SetActive(true); // Afficher la première image de fin
            }
            if (secondGameOverImage != null)
            {
                secondGameOverImage.SetActive(true); // Afficher la deuxième image de fin
            }

            // Couper tous les autres audios et jouer l'audio de fin de batterie
            if (musicAudioSource != null)
            {
                musicAudioSource.Stop(); // Arrêter la musique actuelle
            }

            if (batteryDepletedAudio != null && musicAudioSource != null)
            {
                musicAudioSource.clip = batteryDepletedAudio; // Charger l'audio de fin de batterie
                musicAudioSource.Play(); // Jouer l'audio
            }

            StartCoroutine(RestartSceneAfterDelay(5f)); // Redémarrer la scène après 5 secondes
        }
    }

    void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + money.ToString();
        }
    }

    // Met à jour le texte affichant le prix de recharge
    void UpdateRechargePriceText()
    {
        if (rechargePriceText != null)
        {
            rechargePriceText.text = "Recharge: $" + rechargeCost.ToString(); // Affiche "Prix Recharge: $100"
        }
    }

    // Met à jour le texte affichant le prix de réduction de la décharge de batterie
    void UpdateBatteryDecrementPriceText()
    {
        if (batteryDecrementPriceText != null)
        {
            batteryDecrementPriceText.text = "Reduction Decharge: $" + batteryDecrementCost.ToString(); // Affiche "Prix: $200"
        }
    }

    private IEnumerator ResetHourAndBattery()
    {
        yield return new WaitForSeconds(1f);
        hourCounter = 0f;
        resetHourInProgress = false;
        ResetBattery();
        AddMoney(); // Ajouter de l'argent à la fin de chaque nuit
        nightCounter++;

        // Si on passe à la nuit 2, arrêter la musique de la nuit 1 et changer pour celle de la nuit 2
        if (nightCounter == 5)
        {
            if (musicAudioSource != null)
            {
                musicAudioSource.Stop(); // Arrêter la musique actuelle
                musicAudioSource.clip = night2Music; // Assigner la musique de la nuit 2
                musicAudioSource.Play(); // Démarrer la nouvelle musique
            }
        }

        AdjustBatteryDecrementTime(); // Ajuster la vitesse de décharge de la batterie
        maxAppearancesPerNight += 2; // Augmente le nombre maximum d'apparitions à chaque nuit
        DisplayNightText(); // Met à jour l'affiche de la nuit
        StartCoroutine(DisplayObjectFor6Seconds());
        isWaitingAfterReset = true;

        yield return new WaitForSeconds(6f);

        isWaitingAfterReset = false;
    }

    void ResetBattery()
    {
        battery = 100f;
        batteryResetInProgress = true;
    }

    void DecrementBattery()
    {
        if (battery > 0)
        {
            battery -= (Time.deltaTime / batteryDecrementTime) * 100;
        }
        else
        {
            battery = 0;
        }
    }

    private IEnumerator DisplayObjectFor3Seconds()
    {
        yield return new WaitForSeconds(3f);
        if (objectToDisplay != null)
        {
            objectToDisplay.SetActive(false);
        }
    }

    private IEnumerator DisplayObjectFor6Seconds()
    {
        if (objectToDisplay != null)
        {
            objectToDisplay.SetActive(true);
            yield return new WaitForSeconds(6f);
            objectToDisplay.SetActive(false);
        }
    }

    // Coroutine pour redémarrer la scène après un délai
    private IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Redémarrer la scène actuelle
    }

    void OnToggleButtonClick()
    {
        if (battery < 100f)
        {
            battery += batteryIncreaseAmount;
            if (battery > 100f)
            {
                battery = 100f;
            }
        }
    }

    void DisplayNightText()
    {
        if (nightText != null)
        {
            nightText.text = "Nuit " + nightCounter;
            nightText.gameObject.SetActive(true); // Le texte de la nuit reste toujours affiché
        }
    }

    void AdjustBatteryDecrementTime()
    {
        batteryDecrementTime = Mathf.Max(10f, 100f - (10f * (nightCounter - 1))); // Réduit de 10 secondes chaque nuit, minimum 10 secondes
    }

    void AddMoney()
    {
        int moneyEarned = 100 * (int)Mathf.Pow(2, nightCounter - 1); // Double les gains chaque nuit
        money += moneyEarned;
        UpdateMoneyText(); // Met à jour l'affichage de l'argent
    }

    // Handler pour le clic sur le bouton recharge
    void OnRechargeButtonClick()
    {
        if (money >= rechargeCost)
        {
            money -= rechargeCost; // Déduit le montant de l'argent
            batteryIncreaseAmount += 0.5f; // Augmente la capacité de recharge

            // Multiplier le prix de recharge par 1.5
            rechargeCost = Mathf.RoundToInt(rechargeCost * 2f);

            UpdateMoneyText(); // Met à jour l'affichage de l'argent
            UpdateRechargePriceText(); // Met à jour l'affichage du prix
        }
        else
        {
            Debug.Log("Pas assez d'argent pour acheter !");
        }
    }

    // Handler pour le clic sur le bouton de réduction de la décharge de batterie
    void OnBatteryDecrementButtonClick()
    {
        if (money >= batteryDecrementCost)
        {
            money -= batteryDecrementCost; // Déduit le montant de l'argent
            batteryDecrementTime -= 5f; // Réduit la vitesse de décharge de la batterie (exemple : 5 secondes de moins)

            // Multiplier le prix par 1.5 à chaque achat
            batteryDecrementCost = Mathf.RoundToInt(batteryDecrementCost * 2f);

            UpdateMoneyText(); // Met à jour l'affichage de l'argent
            UpdateBatteryDecrementPriceText(); // Met à jour l'affichage du prix
        }
        else
        {
            Debug.Log("Pas assez d'argent pour acheter !");
        }
    }
}
