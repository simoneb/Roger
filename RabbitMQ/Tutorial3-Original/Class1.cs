namespace Tutorial3_Original
{
    public class Class1
    {
        Consumer c = new Consumer();
        Publisher p = new Publisher();

        public Class1()
        {
            Consumer.Start();
            Publisher.Start();
        }
    }
}