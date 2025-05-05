//using SFD;
//using SFDCT.Helper;
//using Lidgren.Network;
//using HarmonyLib;

//namespace SFDCT.Sync.Custom
//{
//    public static class NetCTData
//    {
//        public struct Data
//        {
//            public Data() { }

//            public NetCTDataType DataType;
//            public object DataObject;
//        }

//        public static Data Read(NetIncomingMessage netIncomingMessage)
//        {
//            Data result = new Data();

//            // ServerUPS is already read before this is read
//            // netIncomingMessage.ReadInt32();
//            result.DataType = (NetCTDataType)netIncomingMessage.ReadInt32();

//            switch (result.DataType)
//            {
//                case NetCTDataType.CTWeapon:
//                    result.DataObject = NetCTWeapon.Read(netIncomingMessage);
//                    break;
//            }

//            return result;
//        }

//        public static NetOutgoingMessage Write(ref Data messageToWrite, NetOutgoingMessage netOutgoingMessage)
//        {
//            NetMessage.WriteDataType(MessageType.ServerPerformance, netOutgoingMessage);
//            netOutgoingMessage.Write(NetMessagesHandler.CTDataReservedServerUPS);

//            netOutgoingMessage.Write((int)messageToWrite.DataType);
//            switch(messageToWrite.DataType)
//            {
//                case NetCTDataType.CTWeapon:
//                    NetCTWeapon.Data ctWeaponData = (NetCTWeapon.Data)messageToWrite.DataObject;
//                    NetCTWeapon.Write(ref ctWeaponData, netOutgoingMessage);
//                    break;
//            }

//            return netOutgoingMessage;
//        }
//    }
//}
