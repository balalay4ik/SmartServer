namespace HomeServer.Domain.Entities;

public enum DeviceWarningType
{
    MacAddressChanged,

    SerialNumberChanged,

    VendorChanged,

    ModelChanged,

    HardwareVersionChanged,

    AdapterChanged,

    ExternalIdConflict,

    DescriptorInvalid
}