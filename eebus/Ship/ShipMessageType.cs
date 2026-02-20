namespace eebus.Ship;

public enum ShipMessageType : byte
{
    Init = 0x00,
    Control = 0x01,
    Data = 0x02,
    End = 0x03
}
