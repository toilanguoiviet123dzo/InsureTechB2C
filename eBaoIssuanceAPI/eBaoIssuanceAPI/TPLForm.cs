using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBaoIssuanceAPI
{
    class ExtInfoModel
    {
        public string infoType = "Motorcycle";
        public int vehicleType = 6;
        public bool coverPA = false;
    }
    class DetailModel
    {
        public int insuredType = 2;
        public ExtInfoModel extInfo = new ExtInfoModel();
        public int vehicleId = 2;
        public string vehicleCountry = "VNM";
        public string vehicleProvince = "10";
        public string vehicleChassisNo = "296948";
        public string vehicleRegNo = "REGNO32333";
        public string vehicleEngineNo = "ENGINENO3351";
        public int vehicleRegYear = 2020;
        public bool isNewVehicle = false;
        public int capacity = 51;
    }
    class InsuredModel
    {
        public DetailModel detail = new DetailModel();
    }
    class PayModeModel
    {
        public string payMode = "credit";
        public ExtInfoModel extInfo;
    }
    class ContactModel
    {
        public string contactNo = "0783500797";
        public int sendTo = 1;
    }
    class DeliveryInfoModel
    {
        public int deliveryMethod = 1;
        public string firstName = "Mike";
        public string lastName = "T";
        public ContactModel extInfo = new ContactModel();
    }
    class CustomerModel
    {
        public class PreferredLangModel
        {
            public int preferredLang = 2;
        }
        public class AddressModel
        {
            public int addressType = 1;
            public string province = "10";
            public string district = "084";
            public string subDistrict = "02815";
            public string postalCode = "02815";
            public string addressLine1 = "strng2";
        }

        public int customerType = 1;
        public PreferredLangModel extInfo = new PreferredLangModel();

        public int idType = 5;
        public string idNo = "98567464";
        public int prefix = 6;
        public string firstName = "Mike";
        public string lastName = "T";
        public string nationality = "THA";
        public string mobile = "0783500797";
        public string email = "mike@ebao.com";
        public string occupation = "1001";
        public string taxNo = "99575656";
        public string branch = "00000";
        public AddressModel address = new AddressModel();

    }
    class PayerModel
    {
        public int payerType = 2;
        public CustomerModel customer = new CustomerModel();
    }
    class PolicyHolderModel
    {
        public bool isSameAsInsured = false;
        public CustomerModel customer = new CustomerModel();
    }
    class TPLForm
    {
        public string insurerTenantCode = "BM_VN";
        public string proposalDate = "15/04/2022 16:30:00";
        public string effDate = "15/04/2022 16:30:00";
        public string expDate = "15/04/2023 16:30:00";
        public string prdtCode = "TPL";
        public string referenceNo = "Test0254";
        public List<InsuredModel> insureds = new List<InsuredModel>();

        public PolicyHolderModel policyholder = new PolicyHolderModel();
        public PayerModel payer = new PayerModel();
        public DeliveryInfoModel deliveryInfo = new DeliveryInfoModel();
        public List<string> documents = new List<string>();
        public PayModeModel payMode = new PayModeModel();
        public ExtInfoModel extInfo;

    }
}
