using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtoChoicer.DB.models
{
    public class BasicInfoReq
    {
        [Required]
        public DateTime 생성시간 { get; set; }
        [Required]
        public string 종목코드 { get; set; }
        [Required]
        public string 종목명 { get; set; }
        [Required]
        public string 종목타입 { get; set; }

        [Required]
        public long 상장주식 { get; set; }
        public int 연중최고 { get; set; }
        public int 연중최저 { get; set; }
        public long 시가총액 { get; set; }
        public double 외인소진률 { get; set; }
        public int 최고250 { get; set; }
        public int 최저250 { get; set; }
        public int 시가 { get; set; }
        public int 고가 { get; set; }
        public int 저가 { get; set; }
        public int 상한가 { get; set; }
        public int 하한가 { get; set; }
        public string 최고가250일 { get; set; }
        public string 최저가250일 { get; set; }
        public double 최고가250대비율 { get; set; }
        public double 최저가250대비율 { get; set; }
        [Required]
        public int 현재가 { get; set; }
        public int 전일대비 { get; set; }
        public double 등락율 { get; set; }
        public int 거래량 { get; set; }
        [Required]
        public long 유통주식 { get; set; }
        public double 유통비율 { get; set; }

    }
}
