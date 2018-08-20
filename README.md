# America's Cup Data Stream Library for .Net

>**Unfortunately, the data service for AC35 has been offline for a while now and this library has no API to connect with. So, we can all look forward to AC36 and the data feeds returning to service.**

This library comes from a love of sailing and the America's Cup organization's forethought and generousity in making the race live data streaming API available to the public. This library is my small way of saying "thank you" to them. In particular, I'd like to thank Ken Milnes for his patience in answering my dumb questions, quickly resolving small issues with the documentation and generally being very helpful.

Usage of the library is extremely simple, perhaps too much so:

``` csharp
    class Program
    {
    	static void Main(string[] args)
    	{
    	    var c = new Client();
    	    var e = new FeedEvents();
    	    e.OnChatterText += ch => Console.WriteLine(string.Format("{0}: {1}", 
    	    	ch.Source, ch.Text));
    	    	
    	    c.OnMessage += e.MessageHandler;
    	    c.Connect();
    	    
    	    Thread.Sleep(Timeout.Infinite);
    	}
    }
```

The above program connects to the data stream server and processes messages.  When a "Chatter Text" message arrives, the `OnChatterText` event is triggered -- this pattern is implemented for all the messages currently available in the API (boat location, race status, etc.).
