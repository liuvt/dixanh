
namespace dixanh.Libraries.Models;

public class BrandLogo
{
    public string lineOne { get; set; }
    public string lineTwo { get; set; }
}

public class DataBrandLogo
{
    public List<BrandLogo> brandLogos { get; set; }

    public DataBrandLogo()
    {
        brandLogos = new List<BrandLogo>(
            new BrandLogo[] {
                new BrandLogo
                {
                    lineOne = "imgs/logosbrand/01casper.jpg",
                    lineTwo = "imgs/logosbrand/08viettel.jpg"
                },
                new BrandLogo
                {
                    lineOne = "imgs/logosbrand/02panasonic.jpg",
                    lineTwo = "imgs/logosbrand/09vinaphone.jpg"
                },
                new BrandLogo{
                    lineOne = "imgs/logosbrand/03sony.jpg",
                    lineTwo = "imgs/logosbrand/010mbbank.jpg"
                },
                new BrandLogo{
                    lineOne = "imgs/logosbrand/04toshiba.jpg",
                    lineTwo = "imgs/logosbrand/011lazada.jpg"
                },
                new BrandLogo{
                    lineOne = "imgs/logosbrand/05lenovo.jpg",
                    lineTwo = "imgs/logosbrand/012shopee.jpg"
                },
                new BrandLogo{
                    lineOne = "imgs/logosbrand/06hp.jpg",
                    lineTwo = "imgs/logosbrand/013tiki.jpg"
                },
                new BrandLogo{
                    lineOne = "imgs/logosbrand/07dell.jpg",
                    lineTwo = "imgs/logosbrand/014thegioididong.jpg"
                },
            }
        );
    }
}