string[] dataItems = new[] { "1", "2", "3" };
IMessageHandlerContext context = default!;

foreach (var item in dataItems)
{
    // Expect highlighting here
    await context.Send<IProcessItem>(m =>
    {
        m.Item = item;
    });
}

var counter = 0;
while (counter < 1)
{
    // Expect highlighting here
    await context.Send<IProcessItem>(m =>
    {
        m.Item = "hello-world";
    });
}

public class MyHandler : IHandleMessages<IProcessItem>
{
    public async Task Handle(IProcessItem message, IMessageHandlerContext context)
    {
        string[] dataItems = new[] { "1", "2", "3" };

        for (int i = 0; i < dataItems.Length; i++)
        {
            // Expect highlighting here
            await context.Publish<IProcessItem>(m =>
            {
                m.Item = dataItems[i];
            });
        }

        var counter = 0;
        while (counter < 1)
        {
            // Expect highlighting here
            await context.Publish<IProcessItem>(m =>
            {
                m.Item = "hello-world";
            });
        }
    }
}

public interface IProcessItem
{
    string Item { get; set; }
}

public interface IMessageHandlerContext
{
    Task Send<T>(Action<T> builder);

    Task Publish<T>(Action<T> builder);
}

public interface IHandleMessages<T>
{
    Task Handle(T message, IMessageHandlerContext context);
}