using Data;

namespace DbMigrations;

public class DatabaseSeeder(FortuneDbContext dbContext)
{
    public async Task SeedDatabase()
    {
        await dbContext.Fortunes.AddRangeAsync(
            new Fortune { Text = "You will write clean code today." },
            new Fortune { Text = "A surprise meeting will inspire you." },
            new Fortune { Text = "Your day will be punctuated by small joys." },
            new Fortune { Text = "Someone from your past will reach out." },
            new Fortune { Text = "An unexpected bug will lead to a breakthrough." },
            new Fortune { Text = "You will discover a shortcut that saves time." },
            new Fortune { Text = "A quiet moment will spark a big idea." },
            new Fortune { Text = "Your persistence will pay off soon." },
            new Fortune { Text = "Collaboration will bring clarity." },
            new Fortune { Text = "Keep an open mind; opportunity strikes." },
            new Fortune { Text = "Your energy attracts positive people." },
            new Fortune { Text = "A small tweak will solve a major issue." },
            new Fortune { Text = "Patience brings better solutions." },
            new Fortune { Text = "A forgotten task resurfaces to help you." },
            new Fortune { Text = "Trust your instincts on the next step." },
            new Fortune { Text = "Clear communication avoids confusion." },
            new Fortune { Text = "Your code will compile on the first try." },
            new Fortune { Text = "Someone will praise your work today." },
            new Fortune { Text = "A random coffee break leads to a new friend." },
            new Fortune { Text = "Every challenge is a chance to learn." }
        );

        await dbContext.SaveChangesAsync();
    }
}
