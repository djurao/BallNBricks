using System;
using TMPro;
using UnityEngine;

namespace Misc
{
    public class HardCurrency : MonoBehaviour
    {
        public static HardCurrency Instance;
        public GameObject purchaseCurrencyPanel;
        public GameObject purchaseSuccessPanel;
        public int amount;
        public CurrencyPackage[] currencyPackages;
        public TextMeshProUGUI currencyLabel;
        void Awake() => Instance = this;
        void Start() => UpdateUILabel();
        public void OpenClosePanel(bool state) => purchaseCurrencyPanel.SetActive(state);
        public void PurchasePackage(int packageID)
        {
            if (!TransactionSuccessful()) return;
            amount += currencyPackages[packageID].amount;
            UpdateUILabel();
            purchaseSuccessPanel.SetActive(true);
        }
        private void UpdateUILabel() => currencyLabel.text = $"{amount}";
        private bool TransactionSuccessful() => true; // Presume that we bought it with real money...
        public void DeductCurrency(int amountToPay)
        {
            amount -= amountToPay;
            UpdateUILabel();        
        }
    }
    [Serializable]
    public class CurrencyPackage
    {
        public int price;
        public int amount;
    }
}