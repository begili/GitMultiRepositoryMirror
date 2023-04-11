using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GitMultiRepositoryMirror.Data
{
    public class BackupInfo
    {

        #region >> Fields <<

        #endregion >> Fields <<

        #region >> Properties <<

        private string _Server = "http://<your git-host domain>";

        public string Server
        {
            get { return _Server; }
            set { _Server = value; }
        }

        private string _TargetPath = @"C:\TargetDirectory";

        public string TargetPath
        {
            get { return _TargetPath; }
            set { _TargetPath = value; }
        }

        public List<RepositoryInfo> Repositories { get; }

        #endregion >> Properties <<

        #region >> CTOR <<

        public BackupInfo()
        {
            Repositories = new List<RepositoryInfo>();
        }

        public BackupInfo(string sampleRepo) : this()
        {
            Repositories.Add(new RepositoryInfo() { RepositorySubPath = sampleRepo });
        }

        #endregion >> CTOR <<

        #region >> Commands <<

        #endregion >> Commands <<

        #region >> Public Methods <<

        public static BackupInfo FromConfigFile(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(BackupInfo));
            FileStream fs = new FileStream(path, FileMode.Open);
            return (BackupInfo)serializer.Deserialize(fs);
        }

        public static void GenerateEmptyConfig(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(BackupInfo));
            TextWriter writer = new StreamWriter(path);
            serializer.Serialize(writer, new BackupInfo("<PATH_TO_EXAMPLE_REPO>"));
            writer.Close();
        }

        #endregion >> Public Methods <<

        #region >> Private Methods <<

        #endregion >> Private Methods <<

        #region >> Override Methods <<

        #endregion >> Override Methods <<

        #region >> Events <<

        #endregion >> Events <<


    }
}
