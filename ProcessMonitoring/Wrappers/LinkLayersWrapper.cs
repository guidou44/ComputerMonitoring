using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Wrappers
{
    public enum LinkLayersWrapper : byte
    {
        Null = 0,
        Ethernet = 1,
        ExperimentalEthernet3MB = 2,
        AmateurRadioAX25 = 3,
        ProteonProNetTokenRing = 4,
        Chaos = 5,
        Ieee802 = 6,
        ArcNet = 7,
        Slip = 8,
        Ppp = 9,
        Fddi = 10,
        RawLegacy = 12,
        SlipBsd = 15,
        PppBsd = 16,
        AtmClip = 19,
        PppOverHdlc = 50,
        Pppoe = 51,
        LlcSnapAtm = 100,
        Raw = 101,
        CiscoHdlc = 104,
        Ieee80211 = 105,
        Loop = 108,
        LinuxSll = 113,
        Ieee80211Radio = 127,
        Ppi = 192
    }
}
