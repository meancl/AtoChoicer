using Microsoft.EntityFrameworkCore;
using AtoChoicer.DB.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace AtoChoicer
{
    public partial class Form1 : Form
    {
        #region 변수
        public const int TR_SCREEN_NUM_START = 1000; // TR 초기화면번호
        public const int TR_SCREEN_NUM_END = 1150; // TR 마지막화면번호

        public long lMillion = 100000000; // 시가총액 곱하기
        public long lThousand = 1000; // 주식수 곱하기

        private int nTrScreenNum = TR_SCREEN_NUM_START;
        public Dictionary<string, BasicInfoReq> infoDict;
        public Dictionary<string, string> typeDict = new Dictionary<string, string>();

        public string[] codeKospiArr;
        public string[] codeKosdaqArr;

        public DateTime today;
        public MJTradierContext dbContext;
        public readonly string NEW_LINE = Environment.NewLine;
        #endregion


        public Form1()
        {
            InitializeComponent(); // c# 고유 고정메소드  

            axKHOpenAPI1.OnEventConnect += OnEventConnectHandler; // 로그인 event slot connect
            axKHOpenAPI1.OnReceiveTrData += OnReceiveTrDataHandler; // TR event slot connect

            this.Text = "Ato Choicer";
            this.KeyPreview = true;
            this.KeyUp += KeyUpHandler;
            
            testTextBox.AppendText("로그인 시도..\r\n");
            dbContext = new MJTradierContext();
            dbContext.Database.EnsureCreated();
            axKHOpenAPI1.CommConnect();
        }


        #region 딜레이 함수
        public void Delay(int ms)
        {
            DateTime dateTimeNow = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);
            DateTime dateTimeAdd = dateTimeNow.Add(duration);
            while (dateTimeAdd >= dateTimeNow)
            {
                System.Windows.Forms.Application.DoEvents();
                dateTimeNow = DateTime.Now;
            }
            return;
        }
        #endregion

        #region  화면번호 관리 
        private string SetTrScreenNo()
        {
            if (nTrScreenNum > TR_SCREEN_NUM_END)
                nTrScreenNum = TR_SCREEN_NUM_START;

            string sTrScreenNum = nTrScreenNum.ToString();
            nTrScreenNum++;
            return sTrScreenNum;
        }
        #endregion

        public void KeyUpHandler(Object sender, KeyEventArgs e)
        {
            char cUp = (char)e.KeyValue; // 대문자로 준다.
            if (cUp == 27)
            {
                this.Close();
                Application.Exit();
            }
        }


        // ============================================
        // 로그인 이벤트발생시 핸들러 메소드
        // ============================================
        public void OnEventConnectHandler(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == 0) // 로그인 성공
            {
                testTextBox.AppendText("로그인 성공\r\n");

                #region  코스피 0, 코스닥 10 종목 받기
                var codeKospiList = axKHOpenAPI1.GetCodeListByMarket("0");
                var codeKosdaqList = axKHOpenAPI1.GetCodeListByMarket("10");

                codeKospiList = codeKospiList.Substring(0, codeKospiList.Length - 1);
                codeKosdaqList = codeKosdaqList.Substring(0, codeKosdaqList.Length - 1);

                codeKospiArr = codeKospiList.Split(';');
                codeKosdaqArr = codeKosdaqList.Split(';');
                #endregion

                #region DB에서 시간에 맞는 종목
                DateTime targetDay;

                if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                    targetDay = DateTime.Today.AddDays(-2);
                else if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday)
                    targetDay = DateTime.Today.AddDays(-1);
                else
                    targetDay = DateTime.Today;

                var yes = DateTime.Today.AddDays(-1); // 어제
                var tod = DateTime.Today; // 오늘

                today = targetDay;

                infoDict = (dbContext.basicInfo.Where(x => x.생성시간.Equals(today)).ToList()).ToDictionary(keySelector: m => m.종목코드, elementSelector: m => m);

                #endregion


                int nShortTerm = 600;
                int nLongTerm = 3600;
                int nTargetTerm = nLongTerm;

                int nTotalMonitored = 0;
                int nTotalMonitorStockNum = codeKospiArr.Length + codeKosdaqArr.Length;
                progressBar1.Maximum = nTotalMonitorStockNum;

                
                StringBuilder sb = new StringBuilder();

                void UpdateProgressBar(ref int nCnt)
                {
                    if (nCnt < nTotalMonitorStockNum)
                    {
                        nCnt++;
                        label1.Text = $"{nCnt} / {nTotalMonitorStockNum} ({Math.Round(nCnt * 100.0 / nTotalMonitorStockNum, 2)}%)";
                        progressBar1.Value = nCnt;
                    }
                    else
                    {
                        testTextBox.AppendText($"종목 초이스가 완료됐습니다.");
                        label1.Text = $"{nCnt} / {nTotalMonitorStockNum} ({Math.Round(nCnt * 100.0 / nTotalMonitorStockNum, 2)}%)";
                        progressBar1.Value = nCnt;
                    }
                }

                for (int i = 0; i < codeKospiArr.Length; i++)
                {
                    typeDict[codeKospiArr[i]] = "KOSPI";
                    if (!infoDict.ContainsKey(codeKospiArr[i]))
                    {
                        if (sb.Length > 0)
                        {
                            testTextBox.Text = sb.ToString();
                            sb.Clear();
                        }
                        
                        testTextBox.AppendText($"코스피  {i + 1}번째 종목 : {codeKospiArr[i]} TR요청{NEW_LINE}");
                        RequestBasicStockInfo(codeKospiArr[i]);
                        Delay(nTargetTerm);
                        UpdateProgressBar(ref nTotalMonitored);
                    }
                    else
                    {
                        nTotalMonitored++;
                        sb.Append($"코스피  {i + 1}번째 종목 : {codeKospiArr[i]} 데이터베이스에 이미 존재합니다.{NEW_LINE}");
                    }    
                }

                for (int i = 0; i < codeKosdaqArr.Length; i++)
                {
                    typeDict[codeKosdaqArr[i]] = "KOSDAQ";
                    if (!infoDict.ContainsKey(codeKosdaqArr[i]))
                    {
                        if (sb.Length > 0)
                        {
                            testTextBox.Text = sb.ToString();
                            sb.Clear();
                        }

                        testTextBox.AppendText($"코스닥  {i + 1}번째 종목 : {codeKosdaqArr[i]} TR요청{NEW_LINE}");
                        RequestBasicStockInfo(codeKosdaqArr[i]);
                        Delay(nTargetTerm);
                        UpdateProgressBar(ref nTotalMonitored);
                    }
                    else
                    {
                        nTotalMonitored++;
                        sb.Append($"코스닥  {i + 1}번째 종목 : {codeKosdaqArr[i]} 데이터베이스에 이미 존재합니다.{NEW_LINE}");
                    }
                }
                UpdateProgressBar(ref nTotalMonitored);

            }
            else
            {
                testTextBox.AppendText("로그인 실패\r\n");
            }
        } // END ---- 로그인 이벤트 핸들러





        // ============================================
        // 주식기본정보요청 TR요청메소드
        // ============================================
        private void RequestBasicStockInfo(string sCode)
        {
            axKHOpenAPI1.SetInputValue("종목코드", sCode);
            int res = axKHOpenAPI1.CommRqData("주식기본정보요청", "opt10001", 0, SetTrScreenNo());
            if (res != 0)
            {
                testTextBox.AppendText($"TR요청이 비정상처리됐습니다{NEW_LINE}");
            }
        }

        // ============================================
        // TR 이벤트발생시 핸들러 메소드
        // ============================================
        private void OnReceiveTrDataHandler(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName.Equals("주식기본정보요청"))
            {
                try
                {
                    string sCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "종목코드").Trim();
                    string sCodeName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "종목명").Trim();
                    long lTotalNumOfStock = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "상장주식")) * lThousand);
                    int nYearlyTop = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "연중최고")));
                    int nYearlyBottom = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "연중최저")));
                    long lMarketCap = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "시가총액")) * lMillion);
                    double fForeignTradeRatio = Math.Abs(double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "외인소진률")));
                    int n250Top = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "250최고")));
                    int n250Bottom = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "250최저")));
                    int nStartPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "시가")));
                    int nHighPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "고가")));
                    int nLowPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "저가")));
                    int nCeilingPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "상한가")));
                    int nFloorPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "하한가")));
                    string s250TopDate = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "250최고가일").Trim();
                    string s250BottomDate = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "250최저가일").Trim();
                    double f250TopRatio = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "250최고가대비율"));
                    double f250BottomRatio = Math.Abs(double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "250최저가대비율")));
                    int nCurPrice = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "현재가")));
                    int nYesterAndTodayDiff = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "전일대비"));
                    double fFluctuationRate = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "등락율"));
                    int nTradeVolume = Math.Abs(int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "거래량")));
                    long lShareOutStanding;
                    double fShareOutStandingRatio;
                    if (axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "유통주식").Trim().Equals(""))
                    {
                        lShareOutStanding = lTotalNumOfStock;
                        fShareOutStandingRatio = 100.0;
                    }
                    else
                    {
                        lShareOutStanding = Math.Abs(long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "유통주식")) * lThousand);
                        fShareOutStandingRatio = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRecordName, 0, "유통비율"));
                    }


                    dbContext.basicInfo.Add(new BasicInfoReq
                    {
                        생성시간 = today,
                        종목코드 = sCode,
                        종목명 = sCodeName,
                        상장주식 = lTotalNumOfStock,
                        연중최고 = nYearlyTop,
                        연중최저 = nYearlyBottom,
                        시가총액 = lMarketCap,
                        외인소진률 = fForeignTradeRatio,
                        최고250 = n250Top,
                        최저250 = n250Bottom,
                        시가 = nStartPrice,
                        고가 = nHighPrice,
                        저가 = nLowPrice,
                        상한가 = nCeilingPrice,
                        하한가 = nFloorPrice,
                        최고가250일 = s250TopDate,
                        최저가250일 = s250BottomDate,
                        최고가250대비율 = f250TopRatio,
                        최저가250대비율 = f250BottomRatio,
                        현재가 = nCurPrice,
                        전일대비 = nYesterAndTodayDiff,
                        등락율 = fFluctuationRate,
                        거래량 = nTradeVolume,
                        유통주식 = lShareOutStanding,
                        유통비율 = fShareOutStandingRatio,
                        종목타입 = typeDict[sCode]
                    });

                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        } // END ---- TR 이벤트 핸들러

    }
}
