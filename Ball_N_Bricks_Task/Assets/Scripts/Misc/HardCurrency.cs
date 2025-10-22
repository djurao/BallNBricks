using System;
using Leaderboard_Module;
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
        private UserCreation _userCreation;
        void Awake() => Instance = this;

        void Start()
        {
            _userCreation = UserCreation.Instance;
            if(_userCreation.user != null )amount = _userCreation.user.hardCurrency;
            UpdateUILabel();
        }

        public void OpenClosePanel(bool state) => purchaseCurrencyPanel.SetActive(state);
        public void PurchasePackage(int packageID)
        {
            if (!TransactionSuccessful()) return;
            amount += currencyPackages[packageID].amount;
            if(_userCreation.user != null ) _userCreation.user.hardCurrency = amount;
            _userCreation.SaveUser();
            UpdateUILabel();
            purchaseSuccessPanel.SetActive(true);
        }
        private void UpdateUILabel() => currencyLabel.text = $"{amount}";
        private bool TransactionSuccessful() => true; // Presume that we bought it with real money...
        public void DeductCurrency(int amountToPay)
        {
            amount -= amountToPay;
            if(_userCreation.user != null ) _userCreation.user.hardCurrency = amount;
            _userCreation.SaveUser();
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