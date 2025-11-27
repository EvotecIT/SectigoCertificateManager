namespace SectigoCertificateManager;

/// <summary>
/// Enumerates known error codes returned by the API.
/// </summary>
public enum ApiErrorCode {
    /// <summary>Unknown error.</summary>
    UnknownError = -1,
    /// <summary>Internal error. Please contact Support for details.</summary>
    InternalErrorPleaseContactSupportForDetails = -2,
    /// <summary>You are not authorized to perform {0}.</summary>
    YouAreNotAuthorizedToPerform0 = -3,
    /// <summary>{0} is required but missing.</summary>
    CodeNeg7 = -7,
    /// <summary>Unknown notification type: {0}</summary>
    UnknownNotificationType0 = -9,
    /// <summary>The CSR is not valid Base-64 data!</summary>
    TheCsrIsNotValidBase64Data = -9,
    /// <summary>Error while decoding CSR.</summary>
    ErrorWhileDecodingCsr = -10,
    /// <summary>The CSR uses an unsupported algorithm!</summary>
    TheCsrUsesAnUnsupportedAlgorithm = -11,
    /// <summary>The CSR uses an unsupported key size!</summary>
    TheCsrUsesAnUnsupportedKeySize = -13,
    /// <summary>Unknown error.</summary>
    UnknownErrorNeg14 = -14,
    /// <summary>Unknown user.</summary>
    UnknownUser = -16,
    /// <summary>You are not authorized to execute {0}</summary>
    YouAreNotAuthorizedToExecute0 = -25,
    /// <summary>The Server type is invalid!</summary>
    TheServerTypeIsInvalid = -35,
    /// <summary>The validity period (term) is invalid for this certificate profile.</summary>
    TheValidityPeriodTermIsInvalidForThisCertificateProfile = -36,
    /// <summary>Access denied.</summary>
    AccessDenied = -37,
    /// <summary>{0}</summary>
    CodeNeg39 = -39,
    /// <summary>The certificate profile id is invalid!</summary>
    TheCertificateProfileIdIsInvalid = -39,
    /// <summary>Internal error while decrypting.</summary>
    InternalErrorWhileDecrypting = -43,
    /// <summary>Error while generating key pair with open SSL</summary>
    ErrorWhileGeneratingKeyPairWithOpenSsl = -44,
    /// <summary>Missing mandatory custom field!</summary>
    MissingMandatoryCustomField = -62,
    /// <summary>Invalid IP address {0}</summary>
    InvalidIpAddress0 = -62,
    /// <summary>Optional field 'name' is invalid!</summary>
    OptionalFieldNameIsInvalid = -64,
    /// <summary>Internal error {0}. Please contact Support for details.</summary>
    InternalError0PleaseContactSupportForDetails = -65,
    /// <summary>KU/EKU template is not allowed for customer.</summary>
    KuekuTemplateIsNotAllowedForCustomer = -76,
    /// <summary>The public key is invalid or not supported.</summary>
    ThePublicKeyIsInvalidOrNotSupported = -78,
    /// <summary>Only issued certificates could be revoked.</summary>
    OnlyIssuedCertificatesCouldBeRevoked = -102,
    /// <summary>Certificate has not been collected yet.</summary>
    CertificateHasNotBeenCollectedYet = -103,
    /// <summary>Person not found.</summary>
    PersonNotFound = -105,
    /// <summary>Error was occurred while renewing cert. Status = {0}</summary>
    ErrorWasOccurredWhileRenewingCertStatus0 = -105,
    /// <summary>Domain Control Validation is either incomplete or expired for {0}. Please complete it before requesting a certificate.</summary>
    DomainControlValidationIsEitherIncompleteOrExpiredFor0PleaseCompleteItBeforeRequestingACertificate = -107,
    /// <summary>Certificate is not available now, please try again later.</summary>
    CertificateIsNotAvailableNowPleaseTryAgainLater = -109,
    /// <summary>Certificate has been revoked and cannot be downloaded.</summary>
    CertificateHasBeenRevokedAndCannotBeDownloaded = -110,
    /// <summary>No certificate profile found by id {0}</summary>
    NoCertificateProfileFoundById0 = -111,
    /// <summary>SSL Certificate to renew is invalid (null)</summary>
    SslCertificateToRenewIsInvalidNull = -123,
    /// <summary>Wrong SSL certificate id {0}.</summary>
    WrongSslCertificateId0 = -124,
    /// <summary>Unknown SSL certificate file format requested: {0}</summary>
    UnknownSslCertificateFileFormatRequested0 = -126,
    /// <summary>Connection error while applying certificate.</summary>
    ConnectionErrorWhileApplyingCertificate = -129,
    /// <summary>SSL state is not ''ISSUED'': {0}</summary>
    SslStateIsNotIssued0 = -130,
    /// <summary>Custom fields limit exceeded for customer.</summary>
    CustomFieldsLimitExceededForCustomer = -131,
    /// <summary>Custom field has to have unique name.</summary>
    CustomFieldHasToHaveUniqueName = -134,
    /// <summary>Custom field cannot be found.</summary>
    CustomFieldCannotBeFound = -135,
    /// <summary>Invalid CSR.</summary>
    InvalidCsr = -138,
    /// <summary>CSR decoding temporarily unavailable. Please try again later.</summary>
    CsrDecodingTemporarilyUnavailablePleaseTryAgainLater = -140,
    /// <summary>The public key size in the CSR should be {0} bits minimum.</summary>
    ThePublicKeySizeInTheCsrShouldBe0BitsMinimum = -141,
    /// <summary>Your certificate already revoked</summary>
    YourCertificateAlreadyRevoked = -159,
    /// <summary>Custom fields limit has been exceeded for this customer. Only {0} custom fields or fewer are allowed.</summary>
    CustomFieldsLimitHasBeenExceededForThisCustomerOnly0CustomFieldsOrFewerAreAllowed = -159,
    /// <summary>You can''t create fields with the same name - {0}!</summary>
    YouCantCreateFieldsWithTheSameName0 = -160,
    /// <summary>Certificate cannot be enrolled for a Local Domain and/or Private IP for a validity period exceeding {0}.</summary>
    CertificateCannotBeEnrolledForALocalDomainAndorPrivateIpForAValidityPeriodExceeding0 = -164,
    /// <summary>Entered data doesn''t match the certificate or no valid certificate found</summary>
    EnteredDataDoesntMatchTheCertificateOrNoValidCertificateFound = -166,
    /// <summary>Certificate is not available, please contact administrator.</summary>
    CertificateIsNotAvailablePleaseContactAdministrator = -169,
    /// <summary>Based on the customer configuration, ECC CSRs are not allowed.</summary>
    BasedOnTheCustomerConfigurationEccCsrsAreNotAllowed = -170,
    /// <summary>The Client Certificate Profile is invalid!</summary>
    TheClientCertificateProfileIsInvalid = -172,
    /// <summary>Updating is not possible. List of your Client Certificate Profile was changed by super admin.</summary>
    UpdatingIsNotPossibleListOfYourClientCertificateProfileWasChangedBySuperAdmin = -176,
    /// <summary>This SSL Certificate Profile doesn''t allow renew</summary>
    ThisSslCertificateProfileDoesntAllowRenew = -180,
    /// <summary>Anchor Certificate details do not match to your request.</summary>
    AnchorCertificateDetailsDoNotMatchToYourRequest = -181,
    /// <summary>Certificate is not collectable.</summary>
    CertificateIsNotCollectable = -183,
    /// <summary>Object has no available customized Client Certificate Profile.</summary>
    ObjectHasNoAvailableCustomizedClientCertificateProfile = -184,
    /// <summary>Customized Client Certificate Profile: {0} has no available terms.</summary>
    CustomizedClientCertificateProfile0HasNoAvailableTerms = -185,
    /// <summary>This user have already reached the maximum allowed number of valid certificates: {0}</summary>
    ThisUserHaveAlreadyReachedTheMaximumAllowedNumberOfValidCertificates0 = -188,
    /// <summary>The CSR uses an unsupported key size.</summary>
    TheCsrUsesAnUnsupportedKeySizeNeg194 = -194,
    /// <summary>CA is not available now. Please try again later.</summary>
    CaIsNotAvailableNowPleaseTryAgainLater = -195,
    /// <summary>Connection error while retrieving DCV email list.</summary>
    ConnectionErrorWhileRetrievingDcvEmailList = -196,
    /// <summary>Old password is incorrect</summary>
    OldPasswordIsIncorrect = -213,
    /// <summary>Cannot change the role of the only {0} user.</summary>
    CannotChangeTheRoleOfTheOnly0User = -219,
    /// <summary>Password can''t be the same.</summary>
    PasswordCantBeTheSame = -220,
    /// <summary>Please select at least one Organization/Department for each selected role</summary>
    PleaseSelectAtLeastOneOrganizationdepartmentForEachSelectedRole = -221,
    /// <summary>Please select roles for the same level</summary>
    PleaseSelectRolesForTheSameLevel = -222,
    /// <summary>Please select only one Organization/Department for each selected role</summary>
    PleaseSelectOnlyOneOrganizationdepartmentForEachSelectedRole = -223,
    /// <summary>This Admin account does not have privileges required to manage ''{0}'' &amp;lt;org&amp;gt;.</summary>
    ThisAdminAccountDoesNotHavePrivilegesRequiredToManage0Org = -226,
    /// <summary>You have no privilege to create this admin user.</summary>
    YouHaveNoPrivilegeToCreateThisAdminUser = -233,
    /// <summary>You have no privilege to modify the privileges of this admin.</summary>
    YouHaveNoPrivilegeToModifyThePrivilegesOfThisAdmin = -234,
    /// <summary>Client Admin''s Email is invalid</summary>
    ClientAdminsEmailIsInvalid = -237,
    /// <summary>You cannot update this client admin which has already been deleted.</summary>
    YouCannotUpdateThisClientAdminWhichHasAlreadyBeenDeleted = -249,
    /// <summary>You have no privilege to modify the role of this admin.</summary>
    YouHaveNoPrivilegeToModifyTheRoleOfThisAdmin = -253,
    /// <summary>Privilege "Allow DCV" can''t be added to non SSL admins.</summary>
    PrivilegeAllowDcvCantBeAddedToNonSslAdmins = -255,
    /// <summary>You have no privilege to assign DCV privileges.</summary>
    YouHaveNoPrivilegeToAssignDcvPrivileges = -256,
    /// <summary>The range is too wide. Maximum of {0} public ip-port pairs and {1} private ip-port pairs per scan are allowed.</summary>
    TheRangeIsTooWideMaximumOf0PublicIpportPairsAnd1PrivateIpportPairsPerScanAreAllowed = -303,
    /// <summary>Incorrect format CIDR.</summary>
    IncorrectFormatCidr = -304,
    /// <summary>The range of ip-port pairs is too wide.</summary>
    TheRangeOfIpportPairsIsTooWide = -305,
    /// <summary>Domain name {0} exceeds {1} characters limit.</summary>
    DomainName0Exceeds1CharactersLimit = -306,
    /// <summary>Customer {0} cannot be found.</summary>
    Customer0CannotBeFound = -410,
    /// <summary>Customer {0} does not have a login name for CA.</summary>
    Customer0DoesNotHaveALoginNameForCa = -429,
    /// <summary>Person name cannot be empty</summary>
    PersonNameCannotBeEmpty = -500,
    /// <summary>You can''t change organization for this person.&amp;lt;br&amp;gt; Key escrow of its level has been enabled for either current organization/department or target organization/department.</summary>
    YouCantChangeOrganizationForThisPersonBrKeyEscrowOfItsLevelHasBeenEnabledForEitherCurrentOrganizationdepartmentOrTargetOrganizationdepartment = -507,
    /// <summary>New person. Please specify name</summary>
    NewPersonPleaseSpecifyName = -508,
    /// <summary>Unknown email address</summary>
    UnknownEmailAddress = -518,
    /// <summary>You have no privilege to modify the email of this person.</summary>
    YouHaveNoPrivilegeToModifyTheEmailOfThisPerson = -524,
    /// <summary>Available Agent(s) are not configured to scan the specified private range(s).</summary>
    AvailableAgentsAreNotConfiguredToScanTheSpecifiedPrivateRanges = -607,
    /// <summary>To scan, you must first enter at least one range parameter.</summary>
    ToScanYouMustFirstEnterAtLeastOneRangeParameter = -615,
    /// <summary>Discovery is currently running. Please try again later.</summary>
    DiscoveryIsCurrentlyRunningPleaseTryAgainLater = -618,
    /// <summary>Available Agent(s) are not configured to scan the specified public range(s).</summary>
    AvailableAgentsAreNotConfiguredToScanTheSpecifiedPublicRanges = -637,
    /// <summary>Supplied orgid invalid..</summary>
    SuppliedOrgidInvalid = -639,
    /// <summary>Such domain already exists</summary>
    SuchDomainAlreadyExists = -700,
    /// <summary>This operation cannot be performed as the delegation status is other than ‘‘Requested’’.</summary>
    ThisOperationCannotBePerformedAsTheDelegationStatusIsOtherThanRequested = -705,
    /// <summary>This domain delegation request has already been deleted.</summary>
    ThisDomainDelegationRequestHasAlreadyBeenDeleted = -707,
    /// <summary>Please delegate domain to at least one organization or department.</summary>
    PleaseDelegateDomainToAtLeastOneOrganizationOrDepartment = -709,
    /// <summary>Domain can''t be delegated to deleted organization.</summary>
    DomainCantBeDelegatedToDeletedOrganization = -711,
    /// <summary>The domain name should be at least {0} characters in length.</summary>
    TheDomainNameShouldBeAtLeast0CharactersInLength = -712,
    /// <summary>The domain name should be at most {0} characters in length.</summary>
    TheDomainNameShouldBeAtMost0CharactersInLength = -713,
    /// <summary>The domain name should have at least {0} dots.</summary>
    TheDomainNameShouldHaveAtLeast0Dots = -714,
    /// <summary>The domain ''{0}'' is inactive.</summary>
    TheDomain0IsInactive = -715,
    /// <summary>&amp;lt;Something&amp;gt; is not a high-level domain. Only high-level domains can be validated.</summary>
    SomethingIsNotAHighlevelDomainOnlyHighlevelDomainsCanBeValidated = -723,
    /// <summary>The request for ''{0}'' cannot be processed since it''s domain validation status is {1}.</summary>
    TheRequestFor0CannotBeProcessedSinceItsDomainValidationStatusIs1 = -724,
    /// <summary>The domain ''{0}'' does not exist.</summary>
    TheDomain0DoesNotExist = -727,
    /// <summary>One or more delegations have been changed by another administrator. Your changes will be ignored.</summary>
    OneOrMoreDelegationsHaveBeenChangedByAnotherAdministratorYourChangesWillBeIgnored = -728,
    /// <summary>You do not have sufficient privileges to modify the name of this domain.</summary>
    YouDoNotHaveSufficientPrivilegesToModifyTheNameOfThisDomain = -731,
    /// <summary>Invalid domain name.</summary>
    InvalidDomainName = -732,
    /// <summary>The domain(s): {0} are not validated! Please perform the DCV process for them before proceed.</summary>
    TheDomains0AreNotValidatedPleasePerformTheDcvProcessForThemBeforeProceed = -737,
    /// <summary>Access denied. You are not allowed to perform the {0} operation on this domain.</summary>
    AccessDeniedYouAreNotAllowedToPerformThe0OperationOnThisDomain = -738,
    /// <summary>This operation cannot be performed due to SSL certificates enrolled for this domain or its subdomains.</summary>
    ThisOperationCannotBePerformedDueToSslCertificatesEnrolledForThisDomainOrItsSubdomains = -740,
    /// <summary>Access denied due to a DRAOs request that has not been approved for domain {0}. Force domain creation is disabled.</summary>
    AccessDeniedDueToDraoRequestThatHasNotBeenApprovedForDomain0ForceDomainCreationIsDisabled = -741,
    /// <summary>The changes of Client Certificate Profile settings will cause the following departments have &amp;lt;br&amp;gt; no available customized Client Certificate Profile, or customized Client Certificate Profiles have no available term or default term: {0}</summary>
    TheChangesOfClientCertificateProfileSettingsWillCauseTheFollowingDepartmentsHaveBrNoAvailableCustomizedClientCertificateProfileOrCustomizedClientCertificateProfilesHaveNoAvailableTermOrDefaultTerm0 = -834,
    /// <summary>The changes of Client Certificate Profile settings will cause the under levels have &amp;lt;br&amp;gt; no available customized Client Certificate Profile, or customized Client Certificate Profiles have no available term or default term.</summary>
    TheChangesOfClientCertificateProfileSettingsWillCauseTheUnderLevelsHaveBrNoAvailableCustomizedClientCertificateProfileOrCustomizedClientCertificateProfilesHaveNoAvailableTermOrDefaultTerm = -840,
    /// <summary>SSL certificate of this type cannot be requested due to ‘{0}’ validation status of the selected organization.</summary>
    SslCertificateOfThisTypeCannotBeRequestedDueTo0ValidationStatusOfTheSelectedOrganization = -843,
    /// <summary>'At least one of the following fields must be filled in: {0}.</summary>
    AtLeastOneOfTheFollowingFieldsMustBeFilledIn0 = -951,
    /// <summary>Incorrect login credentials.</summary>
    IncorrectLoginCredentials = -970,
    /// <summary>New password must be between {0} and 32 characters.</summary>
    NewPasswordMustBeBetween0And32Characters = -976,
    /// <summary>New password length must be 32 characters.</summary>
    NewPasswordLengthMustBe32Characters = -977,
    /// <summary>New password must not contain Login.</summary>
    NewPasswordMustNotContainLogin = -982,
    /// <summary>Domain ''{0}'' is not allowed.</summary>
    Domain0IsNotAllowed = -1010,
    /// <summary>This operation cannot be performed for Organization ''{0}''.</summary>
    ThisOperationCannotBePerformedForOrganization0 = -1021,
    /// <summary>Organization ''{0}'' not found.</summary>
    Organization0NotFound = -1023,
    /// <summary>Invalid order number {0}</summary>
    InvalidOrderNumber0 = -1104,
    /// <summary>No valid client certificates found for {0}.</summary>
    NoValidClientCertificatesFoundFor0 = -1108,
    /// <summary>Certificate can''t be approved cause it has state = {0}</summary>
    CertificateCantBeApprovedCauseItHasState0 = -1112,
    /// <summary>{0} certificate is not ready to be applied.</summary>
    CodeNeg1113 = -1113,
    /// <summary>The SSL is null.</summary>
    TheSslIsNull = -1117,
    /// <summary>The domain(s) {0} have not been validated under the DCV procedure.</summary>
    TheDomains0HaveNotBeenValidatedUnderTheDcvProcedure = -1137,
    /// <summary>Error while checking size of public key in CSR.</summary>
    ErrorWhileCheckingSizeOfPublicKeyInCsr = -1138,
    /// <summary>Since you are a requester of this certificate you can''t approve it. For EV certificates the requester and the approver must not be the same person.</summary>
    SinceYouAreARequesterOfThisCertificateYouCantApproveItForEvCertificatesTheRequesterAndTheApproverMustNotBeTheSamePerson = -1140,
    /// <summary>SSL certificate id: {0} must be re-discovered due to migration need. We are sorry for inconvenience.</summary>
    SslCertificateId0MustBeRediscoveredDueToMigrationNeedWeAreSorryForInconvenience = -1144,
    /// <summary>Replace is forbidden for autoinstalled certificates.</summary>
    ReplaceIsForbiddenForAutoinstalledCertificates = -1148,
    /// <summary>The request is being processed by Sectigo.</summary>
    TheRequestIsBeingProcessedBySectigo = -1400,
    /// <summary>Unsupported certificate format specified: {0}</summary>
    UnsupportedCertificateFormatSpecified0 = -1450,
    /// <summary>Field ''{0}'' has invalid value.</summary>
    Field0HasInvalidValue = -1601,
    /// <summary>Error while validating the domain {0}</summary>
    ErrorWhileValidatingTheDomain0 = -1603,
    /// <summary>DCV is not enabled for this customer.</summary>
    DcvIsNotEnabledForThisCustomer = -1608,
    /// <summary>This {0} was modified or deleted by another user.</summary>
    This0WasModifiedOrDeletedByAnotherUser = -3114,
    /// <summary>This {0} was modified or deleted by another user. Please refresh data.</summary>
    This0WasModifiedOrDeletedByAnotherUserPleaseRefreshData = -3115,
    /// <summary>Invalid scan range: {0}</summary>
    InvalidScanRange0 = -3301,
    /// <summary>You don' t have access to Organization assigned to the Rule</summary>
    YouDonTHaveAccessToOrganizationAssignedToTheRule = -5001,
    /// <summary>Assignment rules cannot be empty.</summary>
    AssignmentRulesCannotBeEmpty = -5002,
    /// <summary>Cannot delete. An assignment rule has been assigned to the Net Discovery Tasks {0}</summary>
    CannotDeleteAnAssignmentRuleHasBeenAssignedToTheNetDiscoveryTasks0 = -5003,
    /// <summary>Certificate not found. {0}</summary>
    CertificateNotFound0 = -5101,
    /// <summary>Device Certificate Profile not found.</summary>
    DeviceCertificateProfileNotFound = -5109,
    /// <summary>Rate limited</summary>
    TooManyRequests = 429,
}
