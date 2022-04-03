using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace PhotoSelector
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private string _ImgDirPath;
        public string ImgDirPath
        {
            get => _ImgDirPath;
            set
            {
                if (_ImgDirPath != value)
                {
                    _ImgDirPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCopying;
        public bool IsCopying
        {
            get => _IsCopying;
            set
            {
                if (_IsCopying != value)
                {
                    _IsCopying = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ProgressMinmum;
        public double ProgressMinmum
        {
            get => _ProgressMinmum;
            set
            {
                if (_ProgressMinmum != value)
                {
                    _ProgressMinmum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ProgressMaximum;
        public double ProgressMaximum
        {
            get => _ProgressMaximum;
            set
            {
                if (_ProgressMaximum != value)
                {
                    _ProgressMaximum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ProgressCur;
        public double ProgressCur
        {
            get => _ProgressCur;
            set
            {
                if (_ProgressCur != value)
                {
                    _ProgressCur = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SelectData _LeftImage;
        public SelectData LeftImage
        {
            get => _LeftImage;
            set
            {
                if (_LeftImage != value)
                {
                    _LeftImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SelectData _RightImage;
        public SelectData RightImage
        {
            get => _RightImage;
            set
            {
                if (_RightImage != value)
                {
                    _RightImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _FindPathCommand;
        public ICommand FindPathCommand
        {
            get
            {
                if (_FindPathCommand == null) _FindPathCommand = new RelayCommand<object>(FindPath);
                return _FindPathCommand;
            }
        }

        private SelectDatas FindAllData { get; set; }
        private SelectDatas _SelectBeforeAllData = new SelectDatas();
        public SelectDatas SelectBeforeAllData 
        {
            get => _SelectBeforeAllData;
            set
            {
                if(_SelectBeforeAllData != value)
                {
                    _SelectBeforeAllData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SelectDatas _SelectAfterAllData = new SelectDatas();
        public SelectDatas SelectAfterAllData
        {
            get => _SelectAfterAllData;
            set
            {
                if (_SelectAfterAllData != value)
                {
                    _SelectAfterAllData = value;
                    RaisePropertyChanged();
                }
            }
        }


        private void FindPath(object obj)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var allowedExtensions = new[] { ".jpg", ".png", ".jpeg", ".bmp", ".gif", ".tif" };
                ImgDirPath = dlg.SelectedPath;

                FindAllData = new SelectDatas(Directory.GetFiles(ImgDirPath)
                    .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
                    .Select(i => new SelectData() { FileFullPath = i }));
                SelectBeforeAllData = new SelectDatas(FindAllData.AsEnumerable());
            }
        }

        private RelayCommand<object> _StartSelectCommand;
        public ICommand StartSelectCommand
        {
            get
            {
                if (_StartSelectCommand == null) _StartSelectCommand = new RelayCommand<object>(StartSelect);
                return _StartSelectCommand;
            }
        }

        private void StartSelect(object obj)
        {
            if(0 < FindAllData?.Count)
            {
                SelectBeforeAllData = new SelectDatas(FindAllData.AsEnumerable());
                ChangeImages();
            }
        }

        private void ChangeImages()
        {
            if(SelectBeforeAllData.Count <= 1 && 2 <= SelectAfterAllData.Count)
            {
                SelectBeforeAllData = SelectAfterAllData;
                SelectAfterAllData.Clear();
            }

            if(2 <= SelectBeforeAllData.Count)
            {
                var random = new Random(DateTime.Now.Millisecond);
                int index1 = 0, index2 = 0;

                index1 = random.Next(SelectBeforeAllData.Count);
                var leftData = SelectBeforeAllData[index1];
                SelectBeforeAllData.RemoveAt(index1);

                index2 = random.Next(SelectBeforeAllData.Count);
                var rightData = SelectBeforeAllData[index2];
                SelectBeforeAllData.RemoveAt(index2);

                LeftImage = leftData;
                RightImage = rightData;
            }
            else if(1 == SelectBeforeAllData.Count)
            {
                System.Windows.MessageBox.Show("사진이 하나 밖에 없습니다.");
            }
            else
            {
                System.Windows.MessageBox.Show("사진 셀렉트를 할 수 없습니다.");
            }
        }

        private RelayCommand<object> _SaveWorkCommand;
        public ICommand SaveWorkCommand
        {
            get
            {
                if (_SaveWorkCommand == null) _SaveWorkCommand = new RelayCommand<object>(SaveWork);
                return _SaveWorkCommand;
            }
        }

        private void SaveWork(object obj)
        {
            try
            {
                List<SelectData> saveData = new List<SelectData>();
                if (0 < SelectBeforeAllData?.Count)
                {
                    saveData.AddRange(SelectBeforeAllData);
                }
                if (LeftImage != null)
                {
                    saveData.Add(LeftImage);
                }
                if (RightImage != null)
                {
                    saveData.Add(RightImage);
                }
                if (0 < SelectAfterAllData?.Count)
                {
                    saveData.AddRange(SelectAfterAllData);
                }

                if (0 < saveData.Count)
                {
                    string jsonData = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                    File.WriteAllText("PhotoSelectorWork.json", jsonData);
                }
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show($"작업을 저장하는데 실패하였습니다. err='{exc}'");
            }
        }

        private RelayCommand<object> _LoadWrokCommand;
        public ICommand LoadWrokCommand
        {
            get
            {
                if (_LoadWrokCommand == null) _LoadWrokCommand = new RelayCommand<object>(LoadWrok);
                return _LoadWrokCommand;
            }
        }

        private void LoadWrok(object obj)
        {
            try
            {
                if(File.Exists("PhotoSelectorWork.json"))
                {
                    string jsonData = File.ReadAllText("PhotoSelectorWork.json");
                    if(!string.IsNullOrEmpty(jsonData))
                    {
                        var selectDatas = JsonConvert.DeserializeObject<List<SelectData>>(jsonData);
                        int higherRunTime = selectDatas.Max(i => i.RunTimes);
                        SelectBeforeAllData = new SelectDatas(selectDatas.Where(i => i.RunTimes < higherRunTime));
                        SelectAfterAllData = new SelectDatas(selectDatas.Where(i => higherRunTime <= i.RunTimes));
                        ChangeImages();
                    }
                }

            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show($"작업을 저장하는데 실패하였습니다. err='{exc}'");
            }
        }

        private RelayCommand<object> _SaveRemainImagesCommand;
        public ICommand SaveRemainImagesCommand
        {
            get
            {
                if (_SaveRemainImagesCommand == null) _SaveRemainImagesCommand = new RelayCommand<object>(SaveRemainImages);
                return _SaveRemainImagesCommand;
            }
        }

        private void SaveImage(SelectDatas saveData, string saveType)
        {
            if (0 < saveData.Count)
            {
                string strMsg = (saveType == "REMAIN") ? "이번 차수에 선택되지 않은" : "지금까지 고른";
                if (System.Windows.MessageBox.Show($"{strMsg} 사진을 따로 저장하시겠습니까?", "알림", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    FolderBrowserDialog dlg = new FolderBrowserDialog();

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (dlg.SelectedPath != ImgDirPath)
                        {
                            foreach (var sel in saveData)
                            {
                                File.Copy(sel.FileFullPath, System.IO.Path.Combine(dlg.SelectedPath, System.IO.Path.GetFileName(sel.FileFullPath)));
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("같은 경로에는 저장 못합니다.");
                        }
                    }
                }
            }
            else
            {
                string errMsg = (saveType == "REMAIN") ? "이번 차수의 이미지가 없습니다." : "셀렉트 된 이미지가 없습니다.";
                System.Windows.MessageBox.Show(errMsg);
            }
        }

        private void SaveRemainImages(object obj)
        {
            SaveImage(SelectBeforeAllData, "REMAIN");
        }

        private RelayCommand<object> _SaveImagesCommand;
        public ICommand SaveImagesCommand
        {
            get
            {
                if (_SaveImagesCommand == null) _SaveImagesCommand = new RelayCommand<object>(SaveImages);
                return _SaveImagesCommand;
            }
        }
        
        private void SaveImages(object obj)
        {
            SaveImage(SelectBeforeAllData, "SELECT");
        }

        private RelayCommand<object> _SelectLeftCommand;
        public ICommand SelectLeftCommand
        {
            get
            {
                if (_SelectLeftCommand == null) _SelectLeftCommand = new RelayCommand<object>(SelectLeft);
                return _SelectLeftCommand;
            }
        }

        private void SelectLeft(object obj)
        {
            if(obj is SelectData data)
            {
                data.RunTimes++;
                SelectAfterAllData.Add(data);
                ChangeImages();
            }
        }

        private RelayCommand<object> _SelectRightCommand;
        public ICommand SelectRightCommand
        {
            get
            {
                if (_SelectRightCommand == null) _SelectRightCommand = new RelayCommand<object>(SelectRight);
                return _SelectRightCommand;
            }
        }

        private void SelectRight(object obj)
        {
            if (obj is SelectData data)
            {
                data.RunTimes++;
                SelectAfterAllData.Add(data);
                ChangeImages();
            }
        }

        private RelayCommand<object> _HoldCommand;
        public ICommand HoldCommand
        {
            get
            {
                if (_HoldCommand == null) _HoldCommand = new RelayCommand<object>(Hold);
                return _HoldCommand;
            }
        }

        private void Hold(object obj)
        {
            if(LeftImage != null && RightImage != null)
            {
                LeftImage.RunTimes++;
                RightImage.RunTimes++;
                SelectAfterAllData.Add(LeftImage);
                SelectAfterAllData.Add(RightImage);
            
                ChangeImages();
            }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private Action<object> mExecuteAct;

        public RelayCommand(Action<object> _executeAct)
        {
            mExecuteAct = _executeAct;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            mExecuteAct(parameter);
        }
    }

    public class SelectData
    {
        public string FileFullPath { get; set; }
        public int RunTimes { get; set; }
    }

    public class SelectDatas : ObservableCollection<SelectData>
    {
        public SelectDatas() : base()
        {
        }

        public SelectDatas(IEnumerable<SelectData> srcData) : base(srcData)
        {
        }
    }
}
