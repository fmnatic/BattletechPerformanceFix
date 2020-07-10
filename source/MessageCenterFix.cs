using Harmony;
using BattleTech.UI;
using System.Collections.Generic;
using static BattletechPerformanceFix.Extensions;
using System.Reflection;
using System;
using System.Linq;
using HBS.Logging;

namespace BattletechPerformanceFix
{
    public class MessageCenterFix : Feature
    {
        //This Feature fixes the messagecenter messagetable using "copy on read" every time a message is sent. Switches the entire thing to "copy on write"
        private static MethodInfo ___GetDelegateGUID_ReceiveMessageCenterMessage;
        private static MethodInfo ___GetDelegateGUID_ReceiveMessageCenterMessageAutoDelete;
        public void Activate()
        {
            ___GetDelegateGUID_ReceiveMessageCenterMessage = typeof(MessageCenter).GetMethod("GetDelegateGUID", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ReceiveMessageCenterMessage) }, null);
            ___GetDelegateGUID_ReceiveMessageCenterMessageAutoDelete = typeof(MessageCenter).GetMethod("GetDelegateGUID", BindingFlags.NonPublic | BindingFlags.Instance,null,new Type[] {typeof(ReceiveMessageCenterMessageAutoDelete) },null);

            {
                var AddSubscriber = Main.CheckPatch(AccessTools.Method(typeof(MessageCenter), nameof(MessageCenter.AddSubscriber))
                                             , "");
                var nAddSubscriber = new HarmonyMethod(AccessTools.Method(typeof(MessageCenterFix), nameof(AddSubscriber)));
                nAddSubscriber.prioritiy = Priority.Last;

                Main.harmony.Patch(AddSubscriber, nAddSubscriber);
            }

            {
                var AddFiniteSubscriber = Main.CheckPatch(AccessTools.Method(typeof(MessageCenter), nameof(MessageCenter.AddFiniteSubscriber))
                                             , "");
                var nAddFiniteSubscriber = new HarmonyMethod(AccessTools.Method(typeof(MessageCenterFix), nameof(AddFiniteSubscriber)));
                nAddFiniteSubscriber.prioritiy = Priority.Last;

                Main.harmony.Patch(AddFiniteSubscriber, nAddFiniteSubscriber);
            }

            
            {
                var RemoveSubscriber = Main.CheckPatch(AccessTools.Method(typeof(MessageCenter), nameof(MessageCenter.RemoveSubscriber))
                                             , "");
                var nRemoveSubscriber = new HarmonyMethod(AccessTools.Method(typeof(MessageCenterFix), nameof(RemoveSubscriber)));
                nRemoveSubscriber.prioritiy = Priority.Last;

                Main.harmony.Patch(RemoveSubscriber, nRemoveSubscriber);
            }

            
            {
                var RemoveFiniteSubscriber = Main.CheckPatch(AccessTools.Method(typeof(MessageCenter), nameof(MessageCenter.RemoveFiniteSubscriber))
                                             , "");
                var nRemoveFiniteSubscriber = new HarmonyMethod(AccessTools.Method(typeof(MessageCenterFix), nameof(RemoveFiniteSubscriber)));
                nRemoveFiniteSubscriber.prioritiy = Priority.Last;

                Main.harmony.Patch(RemoveFiniteSubscriber, nRemoveFiniteSubscriber);
            }
            
            {
                var SendMessagesForType = Main.CheckPatch(AccessTools.Method(typeof(MessageCenter), "SendMessagesForType")
                                             , "");
                var nSendMessagesForType = new HarmonyMethod(AccessTools.Method(typeof(MessageCenterFix), nameof(SendMessagesForType)));
                nSendMessagesForType.prioritiy = Priority.Last;

                Main.harmony.Patch(SendMessagesForType, nSendMessagesForType);
            }

        }

        public static bool AddSubscriber(MessageCenter __instance    ,MessageCenterMessageType GUID, ReceiveMessageCenterMessage subscriber,ref Dictionary<MessageCenterMessageType, List<MessageSubscription>> ___messageTable,ref int ___messageIndex,ref MessageCenterWatcher ___watcher)
        {
            if (!___messageTable.ContainsKey(GUID))
            {
                ___messageTable[GUID] = new List<MessageSubscription>(1);
            }
            else
            {
                List<MessageSubscription> temp = ___messageTable[GUID];
                ___messageTable[GUID] = new List<MessageSubscription>(temp.Count+1);
                ___messageTable[GUID].AddRange(temp);
            }
            List<MessageSubscription> source = ___messageTable[GUID];
            if (source.Any<MessageSubscription>((Func<MessageSubscription, bool>)(ms => ms.Callback == subscriber)))
                return false;
            MessageSubscription messageSubscription = new MessageSubscription()
            {
                Callback = subscriber
            };
            source.Add(messageSubscription);
            if (!((UnityEngine.Object)___watcher != (UnityEngine.Object)null))
                return false;
            string delegateGuid = (string)___GetDelegateGUID_ReceiveMessageCenterMessage.Invoke(__instance   , new object[] { subscriber });
            ___watcher.Add(GUID, delegateGuid);
            return false;
        }

        public static bool AddFiniteSubscriber(MessageCenter __instance   ,
  MessageCenterMessageType GUID,
  ReceiveMessageCenterMessageAutoDelete subscriber, ref Dictionary<MessageCenterMessageType, List<MessageSubscription>> ___messageTable, ref int ___messageIndex, ref MessageCenterWatcher ___watcher)
        {
            if (!___messageTable.ContainsKey(GUID))
            {
                ___messageTable[GUID] = new List<MessageSubscription>(1);
            }
            else
            {
                List<MessageSubscription> temp = ___messageTable[GUID];
                ___messageTable[GUID] = new List<MessageSubscription>(temp.Count + 1);
                ___messageTable[GUID].AddRange(temp);
            }
            List<MessageSubscription> source = ___messageTable[GUID];
            if (source.Any<MessageSubscription>((Func<MessageSubscription, bool>)(ms => ms.DeleteCallback == subscriber)))
                return false;
            MessageSubscription messageSubscription = new MessageSubscription()
            {
                DeleteCallback = subscriber
            };
            source.Add(messageSubscription);
            if (!((UnityEngine.Object)___watcher != (UnityEngine.Object)null))
                return false;
            string delegateGuid = (string)___GetDelegateGUID_ReceiveMessageCenterMessageAutoDelete.Invoke(__instance   , new object[] { subscriber });
            ___watcher.Add(GUID, delegateGuid);
            return false;
        }

        public static bool RemoveSubscriber(MessageCenter __instance   ,
   MessageCenterMessageType GUID,
   ReceiveMessageCenterMessage subscriber, ref Dictionary<MessageCenterMessageType, List<MessageSubscription>> ___messageTable, ref int ___messageIndex, ref MessageCenterWatcher ___watcher)
        {
            if (!___messageTable.ContainsKey(GUID))
                return false;

            ___messageTable[GUID] = new List<MessageSubscription>(___messageTable[GUID]);

            List<MessageSubscription> source = ___messageTable[GUID];
            MessageSubscription messageSubscription = source.FirstOrDefault<MessageSubscription>((Func<MessageSubscription, bool>)(ms => ms.Callback == subscriber));
            if (messageSubscription != null)
            {
                source.Remove(messageSubscription);
                if ((UnityEngine.Object)___watcher != (UnityEngine.Object)null)
                {
                    string delegateGuid = (string)___GetDelegateGUID_ReceiveMessageCenterMessage.Invoke(__instance   , new object[] { subscriber });
                    ___watcher.Rem(GUID, delegateGuid);
                }
            }
            if (source.Count != 0)
                return false;
            ___messageTable.Remove(GUID);
            return false;
        }

        public static bool RemoveFiniteSubscriber(MessageCenter __instance ,
   MessageCenterMessageType GUID,
   ReceiveMessageCenterMessageAutoDelete subscriber, ref Dictionary<MessageCenterMessageType, List<MessageSubscription>> ___messageTable, ref int ___messageIndex, ref MessageCenterWatcher ___watcher)
        {
            if (!___messageTable.ContainsKey(GUID))
                return false;

            ___messageTable[GUID] = new List<MessageSubscription>(___messageTable[GUID]);

            List<MessageSubscription> source = ___messageTable[GUID];
            MessageSubscription messageSubscription = source.FirstOrDefault<MessageSubscription>((Func<MessageSubscription, bool>)(ms => ms.DeleteCallback == subscriber));
            if (messageSubscription != null)
            {
                source.Remove(messageSubscription);
                if ((UnityEngine.Object)___watcher != (UnityEngine.Object)null)
                {
                    string delegateGuid = (string)___GetDelegateGUID_ReceiveMessageCenterMessageAutoDelete.Invoke(__instance   , new object[] { subscriber });
                    ___watcher.Rem(GUID, delegateGuid);
                }
            }
            if (source.Count != 0)
                return false;
            ___messageTable.Remove(GUID);
            return false;
        }

        private static bool SendMessagesForType(MessageCenter __instance ,
    MessageCenterMessageType messageType,
    MessageCenterMessage message, ref Dictionary<MessageCenterMessageType, List<MessageSubscription>> ___messageTable, ref int ___messageIndex, ref MessageCenterWatcher ___watcher)
        {
            List<MessageSubscription> messageSubscriptionList1 = (List<MessageSubscription>)null;
            if (!___messageTable.TryGetValue(messageType, out messageSubscriptionList1))
                return false;
            for (int index = 0; index < messageSubscriptionList1.Count; ++index)
            {
                MessageSubscription messageSubscription = messageSubscriptionList1[index];
                try
                {
                    messageSubscription.Callback?.Invoke(message);
                    if (messageSubscription.DeleteCallback != null)
                    {
                        if (messageSubscription.DeleteCallback(message))
                            RemoveFiniteSubscriber(__instance ,messageType, messageSubscription.DeleteCallback,ref ___messageTable,ref ___messageIndex,ref ___watcher);
                    }
                }
                catch (Exception ex)
                {
                    ILog logger = HBS.Logging.Logger.GetLogger(nameof(MessageCenter));
                    if (logger.IsErrorEnabled)
                        logger.LogError((object)string.Format("CRITICAL ERROR, PLEASE REPORT:\nDelegate {0} for message type {1} failed with exception {2}", messageSubscription != null ? (object)messageSubscription.ToString() : (object)"NULL", (object)messageType.ToString(), (object)string.Format("\n{0}\n{1}", (object)ex.Message, (object)ex.StackTrace)));
                }
            }
            return false;
        }
    }
}
