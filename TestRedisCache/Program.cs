using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using TestRedisCache.Caching;
using TestRedisCache.Models;

namespace TestRedisCache
{
    public class Program
    {

        static void Main(string[] args)
        {
            //rPartneMultiKeyCache is used for Multikey Cache
            RedisMultiKeyCache<Product> rPartneMultiKeyCache =  new RedisMultiKeyCache<Product>("RedisMultiKeyBaseCache", 
                                                                new List<string> { "Id", "ObjectId" },
                                                                TimeSpan.FromMinutes(5),
                                                                ReadProductInfo,
                                                                ReadProductInfoAll);
            //rPartneMemoryCache is normal Memory cache
            RedisMemoryCache<int, Product> rPartneMemoryCache = new RedisMemoryCache<int, Product>("rPartneMemoryCache",        
                                                                TimeSpan.FromMinutes(1), Program.ReadProductInfoFromId);

            RedisMultiHashSetCache<Product> rPartneMulltiHashSetCache = new RedisMultiHashSetCache<Product>("Partner", new List<string>
                                                                        { "Id", "ObjectId" },
                                                                        TimeSpan.FromMinutes(2), 
                                                                        ReadProductInfo,
                                                                        ReadProductInfoAll);

            rPartneMultiKeyCache.Clear();
            var result = "N";
            /*do
            {
                Console.WriteLine(" --------- MultiKey Caching Solution 1-----------");

                Console.WriteLine("Enter Key Class for Product Id/ObjectId: ");
                var productType = Console.ReadLine();
                Console.WriteLine("Enter Value : ");
                var productId = Console.ReadLine();

                var p = rPartneMultiKeyCache[productType, (object)productId];
                if (p != null)
                {
                    Console.WriteLine("Product Details ");

                    Console.WriteLine(" Product Id : " + p.Id);
                            Console.WriteLine(" Product Name : " + p.ProductName);
                            Console.WriteLine(" Product ObjectId : " + p.ObjectId);
                            Console.WriteLine(" Product Type : " + p.ProductType);
                            Console.WriteLine(" Price : " + p.Price);

                }
                else
                    Console.WriteLine("Product Details Not found !");

                //program.ReadDataFromDB();
                Console.WriteLine("Do you want to continue ? (Y/N)");
                result = Console.ReadLine();
            } while (result.ToString().Equals("Y") || result.ToString().Equals("y"));
            */

            //do
            //{
            //    Console.WriteLine(" --------- MultiKey Caching Solution 2-----------");

            //    Console.WriteLine("Enter Key Class for Product Id/ObjectId: ");
            //    var productType = Console.ReadLine();
            //    Console.WriteLine("Enter Value : ");
            //    var productId = Console.ReadLine();

            //    var p = rPartneMulltiHashSetCache[productType, (object)productId];
            //    if (p != null)
            //    {
            //        Console.WriteLine("Product Details ");

            //        Console.WriteLine(" Product Id : " + p.Id);
            //        Console.WriteLine(" Product Name : " + p.ProductName);
            //        Console.WriteLine(" Product ObjectId : " + p.ObjectId);
            //        Console.WriteLine(" Product Type : " + p.ProductType);
            //        Console.WriteLine(" Price : " + p.Price);

            //    }
            //    else
            //        Console.WriteLine("Product Details Not found !");

            //    Console.WriteLine("Do you want to continue ? (Y/N)");
            //    result = Console.ReadLine();
            //} while (result.ToString().Equals("Y") || result.ToString().Equals("y"));


            do
            {
                Console.WriteLine(" --------- Memory Caching -----------");

                Console.WriteLine("Enter Product Id : ");
                var productId = Console.ReadLine();

                var p = rPartneMemoryCache[Convert.ToInt32(productId)];

                if (p != null)
                {
                    Console.WriteLine("Product Details ");

                    Console.WriteLine(" Product Id : " + p.Id);
                    Console.WriteLine(" Product Name : " + p.ProductName);
                    Console.WriteLine(" Product ObjectId : " + p.ObjectId);
                    Console.WriteLine(" Product Type : " + p.ProductType);
                    Console.WriteLine(" Price : " + p.Price);

                }
                else
                    Console.WriteLine("Product Details Not found !");

                Console.WriteLine("Do you want to continue ? (Y/N)");
                result = Console.ReadLine();
            } while (result.ToString().Equals("Y") || result.ToString().Equals("y"));

        }

        public static Product ReadProductInfoFromId(int Key)
        {
            using (var db = new TestDBEntities())
            {
                return db.Products.Where(p => p.Id == Key).FirstOrDefault();
            }
        }

        public static RedisMultiKeyBaseCache<Product>.RedisCacheItem ReadProductInfo(string KeyClass, Object Key)
        {
            using (var db = new TestDBEntities())
            {
                int Id = Convert.ToInt32(Key);
                Product product = new Product();
                if (KeyClass.ToLower().Equals("id"))
                    product = db.Products.Where(p => p.Id == Id).FirstOrDefault();
                else
                    product = db.Products.Where(p => p.ObjectId == Id).FirstOrDefault();

                var lKeys = new Dictionary<string, object> {
                                          {"Id", product.Id},
                                          {"ObjectId", product.ObjectId}
                                        };
                return new RedisMultiKeyBaseCache<Product>.RedisCacheItem(
                  lKeys,
                  product,
                  DateTime.MinValue);
            }
        }

        public static RedisMultiKeyBaseCache<Product>.RedisCacheItem[] ReadProductInfoAll()
        {
            using (var db = new TestDBEntities())
            {
                var products = db.Products.ToList();
                var lRet = new List<RedisMultiKeyBaseCache<Product>.RedisCacheItem>();

                foreach (var product in products)
                {
                    var lKeys = new Dictionary<string, object> {
                                          {"Id", product.Id},
                                          {"Type", product.ProductType}
                                        };

                    lRet.Add(new RedisMultiKeyBaseCache<Product>.RedisCacheItem(
                     lKeys,
                     product,
                     DateTime.MinValue));
                }
                return lRet.ToArray();
            }
        }

    }
}
