using System;
using Silanis.ESL.SDK.Internal;
using Newtonsoft.Json;
using Silanis.ESL.API;

namespace Silanis.ESL.SDK
{
    public class ApprovalService
    {
        private ApprovalApiClient apiClient;
        
        internal ApprovalService(ApprovalApiClient apiClient)
        {
            this.apiClient = apiClient;
        }

        public void DeleteApproval(PackageId packageId, string documentId, string approvalId)
        {
            apiClient.DeleteApproval( packageId.Id, documentId, approvalId );
        }

        public string AddApproval(DocumentPackage sdkPackage, string documentId, Signature signature)
        {
            Approval approval = new SignatureConverter(signature).ToAPIApproval();
            Package apiPackage = new DocumentPackageConverter(sdkPackage).ToAPIPackage();

            if (signature.IsPlaceholderSignature())
            {
                approval.Role = signature.RoleId.Id;
            }
            else if (signature.IsGroupSignature())
            {
                approval.Role = FindRoleIdForGroup(signature.GroupId, apiPackage);
            }
            else
            {
                approval.Role = FindRoleIdForSigner(signature.SignerEmail, apiPackage);
            }

            return apiClient.AddApproval(sdkPackage.Id, documentId, approval);
        }

        public void ModifyApproval(DocumentPackage sdkPackage, string documentId, Signature signature)
        {
            Approval approval = new SignatureConverter(signature).ToAPIApproval();
            Package apiPackage = new DocumentPackageConverter(sdkPackage).ToAPIPackage();

            if (signature.IsPlaceholderSignature())
            {
                approval.Role = signature.RoleId.Id;
            }
            else if (signature.IsGroupSignature())
            {
                approval.Role = FindRoleIdForGroup(signature.GroupId, apiPackage);
            }
            else
            {
                approval.Role = FindRoleIdForSigner(signature.SignerEmail, apiPackage);
            }

            apiClient.ModifyApproval(sdkPackage.Id, documentId, approval);
        }

        public Signature GetApproval(DocumentPackage sdkPackage, string documentId, string approvalId)
        {
            Approval approval = apiClient.GetApproval(sdkPackage.Id, documentId, approvalId);
            Package aPackage = new DocumentPackageConverter(sdkPackage).ToAPIPackage();
            return new SignatureConverter(approval, aPackage).ToSDKSignature();
        }

        public string AddField(PackageId packageId, string documentId, SignatureId signatureId, Field sdkField)
        {
            Silanis.ESL.API.Field apiField = new FieldConverter(sdkField).ToAPIField();
            return apiClient.AddField(packageId, documentId, signatureId, apiField);
        }

        public void ModifyField(PackageId packageId, string documentId, SignatureId signatureId, Field sdkField)
        {
            Silanis.ESL.API.Field apiField = new FieldConverter(sdkField).ToAPIField();
            apiClient.ModifyField(packageId, documentId, signatureId, apiField);
        }

        public void DeleteField(PackageId packageId, string documentId, SignatureId signatureId, string fieldId)
        {
            apiClient.DeleteField(packageId, documentId, signatureId, fieldId);
        }

        private string FindRoleIdForGroup(GroupId groupId, Silanis.ESL.API.Package createdPackage)
        {
            foreach (Silanis.ESL.API.Role role in createdPackage.Roles)
            {
                if (role.Signers.Count > 0 && role.Signers[0].Group != null)
                {
                    if (groupId.Id.Equals(role.Signers[0].Group.Id))
                    {
                        return role.Id;
                    }
                }
            }

            throw new EslException(String.Format("No Role found for group with id {0}", groupId.Id), null);
        }

        private string FindRoleIdForSigner(string signerEmail, Silanis.ESL.API.Package createdPackage)
        {
            foreach (Silanis.ESL.API.Role role in createdPackage.Roles)
            {
                if (role.Signers.Count > 0 && role.Signers[0].Email != null)
                {
                    if (signerEmail.Equals(role.Signers[0].Email))
                    {
                        return role.Id;
                    }
                }
            }

            throw new EslException(String.Format("No Role found for signer email {0}", signerEmail), null);
        }
    }
}

