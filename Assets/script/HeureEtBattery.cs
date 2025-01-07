using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ClockAndBatterySystem : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI batteryText;
    public TextMeshProUGUI nightText; // Texte pour afficher "Nuit X"
    public TextMeshProUGUI moneyText; // Texte pour afficher l'argent
    public TextMeshProUGUI rechargePriceText; // Texte pour afficher le prix du bouton recharge

    private float hourCounter = 0f;
    private float timeUpdateInterval = 10f;
    private float startTime;

    private bool resetHourInProgress = false;
    private bool isWaitingAfterReset = false;

    private float battery = 100f;
    private float batteryDecrementTime = 100f;
    private bool batteryResetInProgress = false;
    private float resetTimer = 0f;
    private float startBatteryTime = 0f;

    public Button toggleButton;
    public GameObject objectToDisplay;
    public Button movingImageButton; // Le bouton qui représentera l'image
    private bool isObjectDisplayed = false;

    public float batteryIncreaseAmount = 10f;
    private int nightCounter = 1; // Compteur pour suivre la nuit actuelle
    private int money = 0; // Montant total d'argent accumulé

    private int maxAppearancesPerNight = 5; // Nombre maximum d'apparitions pour la première nuit

    private int clickCount = 0; // Nombre de clics sur l'image
    private const int maxClicksToMove = 20; // Nombre de clics requis pour déplacer l'image

    public float moveAmount = 10f; // Valeur du déplacement du bouton (modifiable dans l'Inspector)

    private bool isMovingAllowed = true; // Permet de contrôler quand le bouton peut bouger

    // Nouveau bouton recharge
    public Button rechargeButton; // Le bouton qui va permettre d'augmenter le batteryIncreaseAmount
    public int rechargeCost = 100; // Coût de l'achat du recharge

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
        StartCoroutine(HandleMovingImage()); // Commence à gérer les apparitions de l'image

        // Assure-toi que movingImageButton est bien assigné dans l'éditeur Unity
        if (movingImageButton != null)
        {
            movingImageButton.onClick.AddListener(OnMovingImageClick); // Ajoute l'écouteur de clic
        }

        // Ajoute le listener pour le bouton recharge
        if (rechargeButton != null)
        {
            rechargeButton.onClick.AddListener(OnRechargeButtonClick);
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
            rechargePriceText.text = "Prix Recharge: $" + rechargeCost.ToString();
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

    private IEnumerator HandleMovingImage()
    {
        while (true)
        {
            if (hourCounter == 0 || hourCounter == 1 || hourCounter == 5 || hourCounter == 6)
            {
                while (hourCounter == 0 || hourCounter == 1 || hourCounter == 5 || hourCounter == 6)
                {
                    yield return null;
                }
            }

            if (movingImageButton != null)
            {
                movingImageButton.gameObject.SetActive(true);
                yield return new WaitForSeconds(5f);
                movingImageButton.gameObject.SetActive(false);
            }

            float waitTime = Random.Range(5f, 20f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    void OnMovingImageClick()
    {
        clickCount++;

        if (clickCount >= maxClicksToMove && isMovingAllowed)
        {
            Vector3 currentPosition = movingImageButton.transform.position;
            currentPosition.x -= 300f;
            movingImageButton.transform.position = currentPosition;

            clickCount = 0;

            isMovingAllowed = false;
            StartCoroutine(WaitForNextMove());
        }
    }

    private IEnumerator WaitForNextMove()
    {
        yield return new WaitForSeconds(10f);
        isMovingAllowed = true;
    }

    // Handler pour le clic sur le bouton recharge
    void OnRechargeButtonClick()
    {
        if (money >= rechargeCost)
        {
            money -= rechargeCost; // Déduit le montant de l'argent
            batteryIncreaseAmount += 1; // Augmente la capacité de recharge
            UpdateMoneyText(); // Met à jour l'affichage de l'argent
            UpdateRechargePriceText(); // Met à jour l'affichage du prix
        }
        else
        {
            Debug.Log("Pas assez d'argent pour acheter !");
        }
    }
}