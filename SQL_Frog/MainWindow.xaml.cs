using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using 의료IT공학과.데이터베이스;
using Microsoft.Win32;

namespace SQL_Frog
{
    public partial class MainWindow : Window
    {
        public static LocalDB db;
        string sql_Cmd;

        public MainWindow()
        {
            InitializeComponent();
            db = new LocalDB("Provider=Microsoft.ACE.OLEDB.12.0; " + "Data Source=../../../DBFiles/" +
            "STUDENT_ENROLL.accdb; " + "Persist Security Info=False");
            dbName.Content = "STUDENT_ENROLL.accdb";
        }

        // 열기 버튼 클릭 시 발생
        private void OpenDB(object sender, RoutedEventArgs e)
        {
            string path = null;

            OpenFileDialog open = new OpenFileDialog();
            {
                open.CheckFileExists = true;
                open.CheckPathExists = true;

                if(open.ShowDialog().GetValueOrDefault()) 
                {
                    path = open.FileName;  // 선택한 데이터베이스 경로명
                }                
            }

            string[] _dbName = path.Split('\\');
            db = new LocalDB("Provider=Microsoft.ACE.OLEDB.12.0; " + "Data Source=../../../DBFiles/" +
              _dbName[_dbName.Length - 1] + "; " + "Persist Security Info=False");
            dbName.Content = _dbName[_dbName.Length - 1];  // 폼의 데이터베이스 이름 출력            
        }

        private void Change(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            sql_Cmd = box.Text;
        }

        // 내가 입력한 SQL 명령       
        private void Input(object sender, KeyEventArgs e)
        {
            TextBox box = sender as TextBox;
            sql_Cmd = box.Text;

            if (e.Key == Key.Enter)  // Enter키를 누르면 발생
            {
                Play(sender, e);  // 명령어 실행
                queryInput.Select(sql_Cmd.Length, 0);  //커서 끝으로 이동
            }
       
        }

        // 실행 버튼 누르면 발생
        private void Play(object sender, RoutedEventArgs e)
        {
            db.Open();
            string query = sql_Cmd;
            recentList.Items.Add(query);

            // 쿼리 실행
            try { db.Query(query); }
            catch (Exception ex) { MessageBox.Show(query + "\n\n" + ex.Message, "SQL Error"); return; }

            int r = 0;  // 행의 개수를 알기 위함
            DataTable dataTable = new DataTable();
            
            if (db.HasRows)
            {
                DataColumn[] col = new DataColumn[db.FieldCount];  // 열 생성
                for (int i = 0; i < col.Length; i++)
                {       
                    col[i] = new DataColumn(); 
                    col[i].ColumnName = db.GetName(i);  // 열이름 지정
                    dataTable.Columns.Add(col[i]);      // 테이블에 추가
                }

                string [] txt = new string[db.FieldCount];
                
                while (db.Read())
                {
                    DataRow dr = dataTable.NewRow();  // 행 생성
                    for (int i=0; i<db.FieldCount; i++)
                    {
                        dr[i] = db.GetData(i).ToString();  // 데이터 지정
                    }
                    dataTable.Rows.Add(dr);                // 테이블에 추가
                    r++;
                }               
            }
            myGrid.ColumnWidth = this.Width / db.FieldCount;  // 그리드의 가로 폭 지정
            myGrid.RowHeight = myGrid.Height / (r+1);         // 그리드의 세로 길이 지정
            
            myGrid.ItemsSource = dataTable.DefaultView;  // 그리드와 테이블 연동

            db.Close();
        }

        // 리스트 선택 시 queryInput 텍스트박스에 선택 명령 출력
        private void recentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = sender as ListBox;
            int select = recentList.SelectedIndex;
            if (list == null) return;
            if (list.Items.Count == 0) return;
            queryInput.Text = recentList.SelectedItem.ToString();            
            queryInput.Select(queryInput.Text.Length, 0);
            queryInput.Focus();
            sql_Cmd = queryInput.Text;
        }
        
    }
}
