using System;
using System.Text;
using System.Xml.Serialization;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace CodeStack.SwMsgTs.Documents {
    class DataStorageDocHandler : DocumentHandler {
        public class RevData {
            public int Revision { get; set; }
            public Guid RevisionStamp { get; set; }
        }

        private const string STREAM_NAME = "_CodeStackStream_";
        private const string SUB_STORAGE_PATH = "_CodeStackStorage1_\\SubStorage2";
        private const string TIME_STAMP_STREAM_NAME = "TimeStampStream";
        private const string USER_NAME_STREAM_NAME = "UserName";

        private RevData m_RevData;

        public override void OnInit() {
            this.Access3rdPartyData += OnAccess3rdPartyData;

            ShowMessage($"{Model.GetTitle()} document loaded");
        }

        private void OnAccess3rdPartyData(DocumentHandler docHandler, Access3rdPartyDataState_e type) {
            switch(type) {
                case Access3rdPartyDataState_e.StorageRead:
                    LoadFromStorageStore();
                    break;

                case Access3rdPartyDataState_e.StorageWrite:
                    SaveToStorageStore();
                    break;

                case Access3rdPartyDataState_e.StreamRead:
                    LoadFromStream();
                    break;

                case Access3rdPartyDataState_e.StreamWrite:
                    SaveToStream();
                    break;

            }
        }

        public override void OnDestroy() {
            ShowMessage($"{Model.GetTitle()} document destroyed");
        }

        private void SaveToStream() {
            using(var streamHandler = Model.Access3rdPartyStream(STREAM_NAME, true)) {
                using(var str = streamHandler.Stream) {
                    var xmlSer = new XmlSerializer(typeof(RevData));

                    if(m_RevData == null) {
                        m_RevData = new RevData();
                    }

                    m_RevData.Revision = m_RevData.Revision + 1;
                    m_RevData.RevisionStamp = Guid.NewGuid();

                    xmlSer.Serialize(str, m_RevData);
                }
            }
        }

        private void LoadFromStream() {
            using(var streamHandler = Model.Access3rdPartyStream(STREAM_NAME, false)) {
                if(streamHandler.Stream != null) {
                    using(var str = streamHandler.Stream) {
                        var xmlSer = new XmlSerializer(typeof(RevData));
                        m_RevData = xmlSer.Deserialize(str) as RevData;
                        ShowMessage($"Revision data of {Model.GetTitle()}: {m_RevData.Revision} - {m_RevData.RevisionStamp}");
                    }
                } else {
                    ShowMessage($"No revision data stored in {Model.GetTitle()}");
                }
            }
        }

        private void LoadFromStorageStore() {
            var path = SUB_STORAGE_PATH.Split('\\');

            using(var storageHandler = Model.Access3rdPartyStorageStore(path[0], false)) {
                if(storageHandler.Storage != null) {
                    using(var subStorage = storageHandler.Storage.TryOpenStorage(path[1], false)) {
                        foreach(var subStreamName in subStorage.GetSubStreamNames()) {
                            using(var str = subStorage.TryOpenStream(subStreamName, false)) {
                                if(str != null) {
                                    var buffer = new byte[str.Length];

                                    str.Read(buffer, 0, buffer.Length);

                                    var timeStamp = Encoding.UTF8.GetString(buffer);

                                    ShowMessage($"Metadata stamp of {Model.GetTitle()}: {timeStamp}");
                                } else {
                                    ShowMessage($"No metadata stamp stream in {Model.GetTitle()}");
                                }
                            }
                        }
                    }
                } else {
                    ShowMessage($"No metadata storage in {Model.GetTitle()}");
                }
            }
        }

        private void SaveToStorageStore() {
            var path = SUB_STORAGE_PATH.Split('\\');

            using(var storageHandler = Model.Access3rdPartyStorageStore(path[0], true)) {
                using(var subStorage = storageHandler.Storage.TryOpenStorage(path[1], true)) {
                    using(var str = subStorage.TryOpenStream(TIME_STAMP_STREAM_NAME, true)) {
                        var buffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                        str.Write(buffer, 0, buffer.Length);
                    }

                    using(var str = subStorage.TryOpenStream(USER_NAME_STREAM_NAME, true)) {
                        var buffer = Encoding.UTF8.GetBytes(System.Environment.UserName);
                        str.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        private void ShowMessage(string msg) {
            App.SendMsgToUser2(msg,
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
        }
    }
}
