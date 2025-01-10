using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ClockAndBatterySystem : MonoBehaviour
{
    public AudioSource musicAudioSource;
    public AudioSource soundAudioSource;
    public AudioClip night1Music;
    public AudioClip night2Music;
    public AudioClip batteryDepletedAudio;
    public AudioClip buttonClickSound;  // Le son à jouer lorsqu'un bouton est cliqué

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI batteryText;
    public TextMeshProUGUI nightText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI rechargePriceText;
    public TextMeshProUGUI batteryDecrementPriceText;

    public GameObject gameOverImage;
    public GameObject secondGameOverImage;

    private float hourCounter = 0f;
    private float timeUpdateInterval = 10f;
    private float startTime;

    private bool resetHourInProgress = false;
    private bool isWaitingAfterReset = false;

    private float battery = 100f;
    private float batteryDecrementTime = 40f;
    private bool batteryResetInProgress = false;
    private float resetTimer = 0f;
    private float startBatteryTime = 0f;

    public Button toggleButton;
    public GameObject objectToDisplay;

    public float batteryIncreaseAmount = 1f;
    private int nightCounter = 1;
    private int money = 0;

    private int maxAppearancesPerNight = 5;

    public Button rechargeButton;
    public int rechargeCost = 400;

    public Button batteryDecrementButton;
    public int batteryDecrementCost = 2000;

    public GameObject foxy;
    public GameObject foxyDamage;
    private float foxyRandomNumber;
    private bool foxyCanAppear = false;
    private int foxyHealth = 10;

    // Nouveau bouton pour la fin de partie
    public Button endGameButton;
    private int endGameCost = 400000;

    public Button newButton;  // Nouveau bouton pour jouer le son
    public AudioSource buttonClickAudioSource;  // AudioSource pour jouer le son

    void Start()
    {
        startTime = Time.time;
        startBatteryTime = Time.time;

        if (timeText == null) timeText = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        if (batteryText == null) batteryText = GameObject.Find("BatteryText").GetComponent<TextMeshProUGUI>();
        if (nightText == null) nightText = GameObject.Find("NightText").GetComponent<TextMeshProUGUI>();
        if (moneyText == null) moneyText = GameObject.Find("MoneyText").GetComponent<TextMeshProUGUI>();
        if (rechargePriceText == null) rechargePriceText = GameObject.Find("RechargePriceText").GetComponent<TextMeshProUGUI>();
        if (batteryDecrementPriceText == null) batteryDecrementPriceText = GameObject.Find("BatteryDecrementPriceText").GetComponent<TextMeshProUGUI>();

        if (gameOverImage != null) gameOverImage.SetActive(false);
        if (secondGameOverImage != null) secondGameOverImage.SetActive(false);
        if (objectToDisplay != null) objectToDisplay.SetActive(true);

        StartCoroutine(DisplayObjectFor3Seconds());

        if (toggleButton != null) toggleButton.onClick.AddListener(OnToggleButtonClick);

        DisplayNightText();
        UpdateMoneyText();
        UpdateRechargePriceText();
        UpdateBatteryDecrementPriceText();

        StartCoroutine(LaunchFoxy());
        foxy.SetActive(false);
        foxyDamage.SetActive(false);

        if (rechargeButton != null) rechargeButton.onClick.AddListener(OnRechargeButtonClick);
        if (batteryDecrementButton != null) batteryDecrementButton.onClick.AddListener(OnBatteryDecrementButtonClick);

        // Ajout du listener pour le bouton de fin de partie
        if (endGameButton != null) endGameButton.onClick.AddListener(OnEndGameButtonClick);

        // Ajout du listener pour jouer le son lorsqu'on clique sur le nouveau bouton
        if (newButton != null && buttonClickAudioSource != null && buttonClickSound != null)
        {
            newButton.onClick.AddListener(PlayButtonClickSound);
        }

        if (musicAudioSource != null && night1Music != null)
        {
            musicAudioSource.clip = night1Music;
            musicAudioSource.Play();
        }
    }

    // Méthode qui joue le son de clic du bouton
    void PlayButtonClickSound()
    {
        if (buttonClickAudioSource != null && buttonClickSound != null)
        {
            buttonClickAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    void Update()
    {
        if (isWaitingAfterReset) return;

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

            if (hourCounter == 1)
            {
                foxyCanAppear = true;
            }

            if (hourCounter == 5)
            {
                foxyCanAppear = false;
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

        if (foxyHealth <= 0)
        {
            foxy.SetActive(false);
        }

        UpdateTimeText();
        UpdateBatteryText();
    }

    void UpdateTimeText()
    {
        string formattedTime = hourCounter.ToString("00") + ":00";
        if (timeText != null) timeText.text = formattedTime;
    }

    void UpdateBatteryText()
    {
        if (batteryText != null) batteryText.text = Mathf.Round(battery).ToString() + "%";

        if (battery <= 0f)
        {
            if (gameOverImage != null) gameOverImage.SetActive(true);
            if (secondGameOverImage != null) secondGameOverImage.SetActive(true);

            if (musicAudioSource != null)
            {
                musicAudioSource.Stop();
            }

            if (batteryDepletedAudio != null && musicAudioSource != null)
            {
                musicAudioSource.clip = batteryDepletedAudio;
                musicAudioSource.Play();
            }

            StartCoroutine(RestartSceneAfterDelay(5f));
        }
    }

    void UpdateMoneyText()
    {
        if (moneyText != null) moneyText.text = "$" + money.ToString();
    }

    void UpdateRechargePriceText()
    {
        if (rechargePriceText != null)
            rechargePriceText.text = "Recharge: $" + rechargeCost.ToString();
    }

    void UpdateBatteryDecrementPriceText()
    {
        if (batteryDecrementPriceText != null)
            batteryDecrementPriceText.text = "Reduction Decharge: $" + batteryDecrementCost.ToString();
    }

    private IEnumerator ResetHourAndBattery()
    {
        yield return new WaitForSeconds(1f);
        hourCounter = 0f;
        resetHourInProgress = false;
        ResetBattery();
        AddMoney();
        nightCounter++;

        if (nightCounter == 5)
        {
            if (musicAudioSource != null)
            {
                musicAudioSource.Stop();
                musicAudioSource.clip = night2Music;
                musicAudioSource.Play();
            }
        }

        AdjustBatteryDecrementTime();
        maxAppearancesPerNight += 2;
        DisplayNightText();
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
        if (objectToDisplay != null) objectToDisplay.SetActive(false);
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

    private IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private IEnumerator LaunchFoxy()
    {
        yield return new WaitForSeconds(10); //temps d'attente entre chaque possibilite d'apparition en secondes
        foxyRandomNumber = Random.Range(0f, 100f); //ecart entre le plus petit
        if (foxyRandomNumber >= 50f && foxyCanAppear == true) // pourcentage de chance qu'il apparaisse
        {
            foxyHealth = 10;
            foxy.SetActive(true);
            yield return new WaitForSeconds(5);
            foxy.SetActive(false);
            if (foxyHealth > 0)
            {
                battery -= 50;
                foxyDamage.SetActive(true);
                yield return new WaitForSeconds(0.2f);
                foxyDamage.SetActive(false);
            }
        }
        StartCoroutine(LaunchFoxy());
    }

    public void HitFoxy()
    {
        foxyHealth -= 1;
    }

    void OnToggleButtonClick()
    {
        if (battery < 100f)
        {
            battery += batteryIncreaseAmount;
            if (battery > 100f) battery = 100f;
        }
    }

    void DisplayNightText()
    {
        if (nightText != null) nightText.text = "Nuit " + nightCounter;
    }

    void AdjustBatteryDecrementTime()
    {
        batteryDecrementTime = Mathf.Max(10f, 100f - (10f * (nightCounter - 1)));
    }

    void AddMoney()
    {
        int moneyEarned = 100 * (int)Mathf.Pow(2, nightCounter - 1);
        money += moneyEarned;
        UpdateMoneyText();
    }

    void OnRechargeButtonClick()
    {
        if (money >= rechargeCost)
        {
            money -= rechargeCost;
            batteryIncreaseAmount += 0.5f;

            rechargeCost = Mathf.RoundToInt(rechargeCost * 2f);

            UpdateMoneyText();
            UpdateRechargePriceText();
        }
    }

    void OnBatteryDecrementButtonClick()
    {
        if (money >= batteryDecrementCost)
        {
            money -= batteryDecrementCost;
            batteryDecrementTime += 5f;

            batteryDecrementCost = Mathf.RoundToInt(batteryDecrementCost * 2f);

            UpdateMoneyText();
            UpdateBatteryDecrementPriceText();
        }
    }

    void OnEndGameButtonClick()
    {
        if (money >= endGameCost)
        {
            money -= endGameCost;
            UpdateMoneyText();

            StartCoroutine(EndGame());
        }
        else
        {
            Debug.Log("Pas assez d'argent pour acheter la fin de partie !");
        }
    }

    private IEnumerator EndGame()
    {
        soundAudioSource.PlayOneShot(batteryDepletedAudio);
        yield return new WaitForSeconds(2f);
        Debug.Log("Fin de Partie achetée !");
        // Charge la scène VictoryScene lorsque le joueur achète la fin de partie
        SceneManager.LoadScene("win"); // Remplacez "VictoryScene" par le nom de la scène que vous souhaitez charger
    }
}

