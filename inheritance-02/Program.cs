using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace inheritance_02
{
    static class Program
    {
        static void Main(string[] args)
        {
            //Выведите платёжные ссылки для трёх разных систем платежа: 
            //pay.system1.ru/order?amount=12000RUB&hash={MD5 хеш ID заказа}
            //order.system2.ru/pay?hash={MD5 хеш ID заказа + сумма заказа}
            //system3.com/pay?amount=12000&curency=RUB&hash={SHA-1 хеш сумма заказа + ID заказа + секретный ключ от системы}

            Order order = new Order(12459, 1200);
            
            SystemOne systemOne = new SystemOne();
            Console.WriteLine(systemOne.GetPayingLink(order));
            SystemTwo systemTwo = new SystemTwo();
            Console.WriteLine(systemTwo.GetPayingLink(order));
            SystemThree systemThree = new SystemThree();
            Console.WriteLine(systemThree.GetPayingLink(order));
        }
    }

    public class Order
    {
        public readonly int Id;
        public readonly int Amount;

        public Order(int id, int amount) => (Id, Amount) = (id, amount);
    }

    public interface IPaymentSystem
    {
        string GetPayingLink(Order order);
    }
    
    public class SystemOne : IPaymentSystem
    {
        private readonly string _link;

        public SystemOne()
        {
            _link = "pay.system1.ru/order?amount=12000RUB&hash=";
        }

        public string GetPayingLink(Order order)
        {
            var hashId = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(order.Id.ToString()));
            return _link + Convert.ToBase64String(hashId);
        }
    }

    public class SystemTwo : IPaymentSystem
    {
        private readonly string _link;

        public SystemTwo()
        {
            _link = "order.system2.ru/pay?hash=";
        }

        public string GetPayingLink(Order order)
        {
            var hashId = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(order.Id.ToString()));
            return _link + Convert.ToBase64String(hashId) + order.Amount;
        }
    }

    public class SystemThree : IPaymentSystem
    {
        private readonly string _link;
        private readonly string _secretKey;
        
        public SystemThree()
        {
            _link = "system3.com/pay?amount=12000&curency=RUB&hash=";
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            _secretKey = Convert.ToBase64String(time.Concat(key).ToArray());
        }

        public string GetPayingLink(Order order)
        {
            var hashAmount = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(order.Amount.ToString()));
            return _link + Convert.ToBase64String(hashAmount) + order.Id + _secretKey;
        }
    }
}