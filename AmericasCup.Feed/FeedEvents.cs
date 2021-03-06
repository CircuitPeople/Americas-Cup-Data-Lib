﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmericasCup.Data;
using AmericasCup.Data.Xml;
using System.Xml.Serialization;

namespace AmericasCup.Feed
{
   public class FeedEvents
   {
      public FeedEvents() { InitializeMessageHandlers(); }

      public Dictionary<MessageTypeEnum, Action<byte[], byte[]>> MessageTypeHandlers;

      public void InitializeMessageHandlers()
      {
         MessageTypeHandlers = new Dictionary<MessageTypeEnum, Action<byte[], byte[]>>()
            {
                { MessageTypeEnum.Heartbeat, (header, body) => OnHeartbeat?.Invoke(Heartbeat.Read(body)) },
                { MessageTypeEnum.RaceStatus, (header, body) => OnRaceStatus?.Invoke(RaceStatus.Read(body)) },
                { MessageTypeEnum.DisplayTextMessage, (header, body) => OnDisplayTextMessage?.Invoke(DisplayTextMessage.Read(body)) },
                { MessageTypeEnum.XmlMessage, (header, body) => HandleXmlMessage(XmlMessage.Read(body)) },
                { MessageTypeEnum.RaceStartStatus, (header, body) => OnRaceStartStatus?.Invoke(RaceStartStatus.Read(body)) },
                { MessageTypeEnum.YachtEventCode, (header, body) => OnYachtEventCode?.Invoke(YachtEventCode.Read(body)) },
                { MessageTypeEnum.YachtActionCode, (header, body) => OnYachtActionCode?.Invoke(YachtActionCode.Read(body)) },
                { MessageTypeEnum.ChatterText, (header, body) => OnChatterText?.Invoke(ChatterText.Read(body)) },
                { MessageTypeEnum.BoatLocation, (header, body) => OnBoatLocation?.Invoke(BoatLocation.Read(body)) },
                { MessageTypeEnum.MarkRounding, (header, body) => OnMarkRounding?.Invoke(MarkRounding.Read(body)) },
                { MessageTypeEnum.CourseWind, (header,body) => OnCourseWind?.Invoke(CourseWind.Read(body)) },
                { MessageTypeEnum.AverageWind, (header,body) => OnAverageWind?.Invoke(AverageWind.Read(body)) }
            };
      }

      public void MessageHandler(byte[] header, byte[] body, byte[] crc)
      {
         var type = (MessageTypeEnum)header[2];

         if (MessageTypeHandlers.ContainsKey(type))
            MessageTypeHandlers[type](header, body);
         else if (OnUnsupportedMessage != null)
         {
            var msg = new Message();
            MessageHeader.Read(header, 0, msg.Header);
            Message.Read(body, 0, msg);
            msg.Crc = BitConverter.ToUInt32(crc, 0);

            OnUnsupportedMessage(msg);
         }
      }

      static Dictionary<Type, XmlSerializer> _serializers = new Dictionary<Type, XmlSerializer>();
      void HandleXmlMessage<T>(string text, Action<T> action) where T : class
      {
         if (action != null)
         {
            var t = typeof(T);
            var s = _serializers.ContainsKey(t) ? _serializers[t] : _serializers[t] = new XmlSerializer(t);
            var xml = s.Deserialize(new System.IO.StringReader(text)) as T;
            action(xml);
         }
      }

      void HandleXmlMessage(XmlMessage m)
      {
         switch (m.SubType)
         {
            case XmlMessageSubTypeEnum.Regatta:
               HandleXmlMessage(m.Text, OnRegattaConfig);
               break;
            case XmlMessageSubTypeEnum.Race:
               HandleXmlMessage(m.Text, OnRaceConfig);
               break;
            case XmlMessageSubTypeEnum.Boat:
               HandleXmlMessage(m.Text, OnBoatConfig);
               break;
            default:
               OnUnsupportedXmlMessage?.Invoke(m);
               break;
         }
      }

      public event Action<RegattaConfig> OnRegattaConfig;
      public event Action<RaceConfig> OnRaceConfig;
      public event Action<BoatConfig> OnBoatConfig;

      public event Action<Heartbeat> OnHeartbeat;
      public event Action<RaceStatus> OnRaceStatus;
      public event Action<DisplayTextMessage> OnDisplayTextMessage;
      public event Action<RaceStartStatus> OnRaceStartStatus;
      public event Action<YachtEventCode> OnYachtEventCode;
      public event Action<YachtActionCode> OnYachtActionCode;
      public event Action<ChatterText> OnChatterText;
      public event Action<BoatLocation> OnBoatLocation;
      public event Action<MarkRounding> OnMarkRounding;
      public event Action<CourseWind> OnCourseWind;
      public event Action<AverageWind> OnAverageWind;

      public event Action<Message> OnUnsupportedMessage;
      public event Action<XmlMessage> OnUnsupportedXmlMessage;
   }
}
