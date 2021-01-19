using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace mylib.FilterCriteria
{
    /// <summary>
    /// TvProgramHierarchicalFilter および TvProgramHierarchicalFilterFolder からなる、階層フィルター ツリーを、指定のフォルダー以下に XML ファイルとして保存、もしくは、フォルダー以下の XML ファイルからツリーを復元します。
    /// </summary>
    class HierarchicalFilterSerializer<T>
    {
        private static string IDS_FILENAME = "ids.xml";
        private static string HTYPES_FILENAME = "htypes.xml";
        private static string TYPES_FILENAME = "types.xml";
        private static string DIRECTORY_NAME = "Filter";

        private string _DirectoryPath;
        private string outputDirectory;
        /// <summary>
        /// シリアライズ先、デシリアライズ元であるフォルダーのパス
        /// </summary>
        public string DirectoryPath
        {
            get { return this._DirectoryPath; }
            set
            {
                this._DirectoryPath = value;
                this.outputDirectory = value + Path.DirectorySeparatorChar + HierarchicalFilterSerializer<T>.DIRECTORY_NAME;
            }
        }

        private int _BackupGeneration;
        /// <summary>
        /// シリアライズ時にバックアップとして残す世代数
        /// </summary>
        public int BackupGeneration
        {
            get { return this._BackupGeneration; }
            set { this._BackupGeneration = value; }
        }

        /// <summary>
        /// 階層フィルター ツリーを、DirectoryPath で指定するフォルダー以下に XML ファイルとして保存します。
        /// </summary>
        /// <param name="filterTree">XML ファイルとして保存する階層フィルター ツリー</param>
        public void Serialize(HierarchicalFilter<T> filterTree)
        {
            if (this.outputDirectory == null) throw new InvalidOperationException("DirectoryPath が未設定です。");

            // ID (ファイル名) の一覧。-1 はサブ フィルタの処理に移る、-2 は親フィルタの処理に戻る。
            List<int> ids = new List<int>();

            // フィルタ種別の一覧。1 は TvProgramStandardFilter、2 は TvProgramResudueFilter、3 は TvProgramFolderFilter。
            List<int> types = new List<int>();

            // 階層フィルタ種別の一覧。1 は TvProgramHierarchicalFilter。
            List<int> htypes = new List<int>();

            // バックアップ処理
            for (int i = BackupGeneration - 1; i >= 0; i--)
            {
                while (Directory.Exists(this.outputDirectory + "." + i))
                {
                    Directory.Move(this.outputDirectory + "." + i, this.outputDirectory + "." + (i + 1));
                    System.Threading.Thread.Sleep(10);
                }
            }
            while (Directory.Exists(this.outputDirectory))
            {
                Directory.Move(this.outputDirectory, this.outputDirectory + ".0");
                System.Threading.Thread.Sleep(10);
            }

            // 保存先フォルダの生成
            DirectoryInfo settingDir = Directory.CreateDirectory(this.outputDirectory);
            System.Threading.Thread.Sleep(10);
            while (!Directory.Exists(this.outputDirectory))
            {
                settingDir = Directory.CreateDirectory(this.outputDirectory);
                System.Threading.Thread.Sleep(10);
            }

            // フィルター ツリーの再帰的シリアライズ
            this.serializeFilterTree(0, ids, types, htypes, filterTree);

            // ID 一覧のシリアライズ
            XmlSerializer serializer = new XmlSerializer(ids.GetType());
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(
                        this.outputDirectory + Path.DirectorySeparatorChar + IDS_FILENAME,
                        false,
                        new System.Text.UTF8Encoding(false)
                        );

                serializer.Serialize(sw, ids);
            }
            finally
            {
                if (sw != null) sw.Close();
                sw = null;
                serializer = null;
            }

            // 階層フィルター型一覧のシリアライズ
            serializer = new XmlSerializer(types.GetType());
            try
            {
                sw = new StreamWriter(
                        this.outputDirectory + Path.DirectorySeparatorChar + HTYPES_FILENAME,
                        false,
                        new System.Text.UTF8Encoding(false)
                        );

                serializer.Serialize(sw, htypes);
            }
            finally
            {
                if (sw != null) sw.Close();
                sw = null;
                serializer = null;
            }

            // フィルター型一覧のシリアライズ
            serializer = new XmlSerializer(types.GetType());
            try
            {
                sw = new StreamWriter(
                        this.outputDirectory + Path.DirectorySeparatorChar + TYPES_FILENAME,
                        false,
                        new System.Text.UTF8Encoding(false)
                        );

                serializer.Serialize(sw, types);
            }
            finally
            {
                if (sw != null) sw.Close();
                sw = null;
                serializer = null;
            }

            // バックアップの削除
            string oldBackupPath = this.outputDirectory + "." + this.BackupGeneration;
            while (Directory.Exists(oldBackupPath))
            {
                Directory.Delete(oldBackupPath, true);
                System.Threading.Thread.Sleep(10);
            }
        }

        #region Private Method for Serialize
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">連番。ファイル名として利用される。</param>
        /// <param name="ids">id の List</param>
        /// <param name="types">フィルターの型を示す数値</param>
        /// <param name="htypes">階層フィルターの型を示す数値</param>
        /// <param name="filterTree">シリアライズする階層フィルター ツリー</param>
        /// <returns></returns>
        private int serializeFilterTree(int id, List<int> ids, List<int> types, List<int> htypes, IHierarchicalFilter<T> filterTree)
        {
            this.serializeFilter(id, ids, types, htypes, (IHierarchicalFilter<T>)filterTree);
            id++;

            foreach (IHierarchicalFilter<T> item in filterTree.Children)
            {
                ids.Add(-1);
                types.Add(0);
                htypes.Add(0);
                id = serializeFilterTree(id, ids, types, htypes, item);
                ids.Add(-2);
                types.Add(0);
                htypes.Add(0);
            }

            return id;
        }

        private void serializeFilter(int id, List<int> ids, List<int> types, List<int> htypes, IHierarchicalFilter<T> hFilter)
        {
            Type hFilterType = hFilter.GetType();
            Type filterType = hFilter.OwnedFilter.GetType();

            if (hFilterType == typeof(HierarchicalFilter<T>))
            {
                htypes.Add(1);
            }
            else
            {
                htypes.Add(-1);
            }

            if (filterType == typeof(StringFilter))
            {
                types.Add(1);
            }
            else if (filterType == typeof(ResudueFilter<T>))
            {
                types.Add(2);
            }
            else if (filterType == typeof(FolderFilter<T>))
            {
                types.Add(3);
            }
            else
            {
                types.Add(-1);
            }

            ids.Add(id);

            if (htypes[htypes.Count - 1] == 2)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(string));
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(
                            this.outputDirectory + Path.DirectorySeparatorChar + id + ".xml",
                            false,
                            new System.Text.UTF8Encoding(false)
                            );

                    serializer.Serialize(sw, hFilter.Name);
                }
                finally
                {
                    if (sw != null) sw.Close();
                    sw = null;
                    serializer = null;
                }
            }
            else if (types[types.Count - 1] != -1)
            {
                XmlSerializer serializer = new XmlSerializer(filterType);
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(
                            this.outputDirectory + Path.DirectorySeparatorChar + id + ".xml",
                            false,
                            new System.Text.UTF8Encoding(false)
                            );

                    serializer.Serialize(sw, hFilter.OwnedFilter);
                }
                finally
                {
                    if (sw != null) sw.Close();
                    sw = null;
                    serializer = null;
                }
            }
        }
        #endregion Private Method for Serialize

        /// <summary>
        /// DirectoryPath で指定するフォルダー以下に XML ファイルとして保存された階層フィルター ツリーを復元します。
        /// </summary>
        /// <returns>復元した階層フィルター ツリー</returns>
        public IHierarchicalFilter<T> Deserialize()
        {
            // フィルター ルートの作成
            IHierarchicalFilter<T> result = new HierarchicalFilter<T>();

            //*
            // ディレクトリ設定のチェック
            if (this.outputDirectory == null) throw new InvalidOperationException("DirectoryPath が未設定です。");

            // ID (ファイル名) の一覧。-1 はサブ フィルタの処理に移る、-2 は親フィルタの処理に戻る。
            List<int> ids = null;

            // 階層フィルタ種別の一覧。1 は TvProgramHierarchicalFilter、2 は TvProgramHierarchicalFilterFolder。
            List<int> htypes = null;

            // フィルタ種別の一覧。1 は TvProgramStandardFilter、2 は TvProgramResudueFilter。
            List<int> types = null;

            // ID 一覧のデシリアライズ
            XmlSerializer serializer = new XmlSerializer(typeof(List<int>));
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(
                        this.outputDirectory + Path.DirectorySeparatorChar + IDS_FILENAME,
                        new System.Text.UTF8Encoding(false)
                        );

                ids = (List<int>)serializer.Deserialize(sr);
            }
            finally
            {
                if (sr != null) sr.Close();
                sr = null;
                serializer = null;
            }

            // 階層フィルター型一覧のデシリアライズ
            serializer = new XmlSerializer(typeof(List<int>));
            try
            {
                sr = new StreamReader(
                        this.outputDirectory + Path.DirectorySeparatorChar + HTYPES_FILENAME,
                        new System.Text.UTF8Encoding(false)
                        );

                htypes = (List<int>)serializer.Deserialize(sr);
            }
            finally
            {
                if (sr != null) sr.Close();
                sr = null;
                serializer = null;
            }


            // フィルター型一覧のデシリアライズ
            serializer = new XmlSerializer(typeof(List<int>));
            try
            {
                sr = new StreamReader(
                        this.outputDirectory + Path.DirectorySeparatorChar + TYPES_FILENAME,
                        new System.Text.UTF8Encoding(false)
                        );

                types = (List<int>)serializer.Deserialize(sr);
            }
            finally
            {
                if (sr != null) sr.Close();
                sr = null;
                serializer = null;
            }
            // フィルター ツリーのデシリアライズ
            result.OwnedFilter = this.deserializeFilter(ids[0], types[0]);

            IHierarchicalFilter<T> newNode = result;
            IHierarchicalFilter<T> currentNode = null;
            for (int i = 1; i < ids.Count; i++)
            {
                //System.Windows.MessageBox.Show(
                //"cur; " + (currentNode == null ? "(null)" : currentNode.Name) + "\n" +
                //"new: " + (newNode == null ? "(null)" : newNode.Name) + "\n" +
                //"id: " + ids[i]
                //);
                if (ids[i] == -1)
                {
                    currentNode = newNode;
                }
                else if (ids[i] == -2)
                {
                    newNode = (IHierarchicalFilter<T>)newNode.Parent;
                }
                else
                {
                    if (htypes[i] == 1)
                    {
                        newNode = new HierarchicalFilter<T>() { OwnedFilter = this.deserializeFilter(ids[i], types[i]) };
                    }
                    else if (htypes[i] == 2)
                    {
                        //newNode = new HierarchicalFilter<T>() { Filter = new FolderFilter<T>() { Name = deserializeString(ids[i]) } };
                        newNode = new HierarchicalFilter<T>() { OwnedFilter = new FolderFilter<T>() };
                    }
                    else
                    {
                        throw new NotImplementedException("未知の階層フィルター形式を指定されました。");
                    }

                    currentNode.Children.Add(newNode);
                }
            }
            // */
            return result;
        }

        #region Private Method for Deserialize
        private IOwnedFilter<T> deserializeFilter(int id, int type)
        {
            XmlSerializer serializer = null;
            IOwnedFilter<T> result = null;

            switch (type)
            {
                case 1:
                    serializer = new XmlSerializer(typeof(StringFilter));
                    break;
                case 2:
                    serializer = new XmlSerializer(typeof(ResudueFilter<T>));
                    break;
                case 3:
                    serializer = new XmlSerializer(typeof(FolderFilter<T>));
                    break;
                default:
                    throw new NotImplementedException("未知のフィルター形式を指定されました。");
            }

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(
                        this.outputDirectory + Path.DirectorySeparatorChar + id + ".xml",
                        new System.Text.UTF8Encoding(false)
                        );

                result = (IOwnedFilter<T>)serializer.Deserialize(sr);
            }
            finally
            {
                if (sr != null) sr.Close();
                sr = null;
                serializer = null;
            }

            return result;
        }

        private string deserializeString(int id)
        {
            XmlSerializer serializer = null;
            string result = null;

            serializer = new XmlSerializer(typeof(string));

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(
                        this.outputDirectory + Path.DirectorySeparatorChar + id + ".xml",
                        new System.Text.UTF8Encoding(false)
                        );

                result = (string)serializer.Deserialize(sr);
            }
            finally
            {
                if (sr != null) sr.Close();
                sr = null;
                serializer = null;
            }

            return result;
        }
        #endregion Private Method for Deserialize
    }
}
