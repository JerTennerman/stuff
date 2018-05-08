using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace dbdroll_bot
{
    public partial class Form1 : Form
    {
        BackgroundWorker bw;
        Random roll = new Random();
        bool toRoll = false;
        Dictionary<string, string> money = new Dictionary<string, string>();
        Dictionary<string, int> exp = new Dictionary<string, int>();
        private string inputKey = "";
        private string inputValue = "";
        private string userKey = "";

        public Form1()
        {
            InitializeComponent();
            this.bw = new BackgroundWorker();
            this.bw.DoWork += bw_DoWork;
        }

        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var key = e.Argument as String;
            try
            {
                var Bot = new Telegram.Bot.TelegramBotClient(key);
                await Bot.SetWebhookAsync("");

                Bot.OnUpdate += async (object su, Telegram.Bot.Args.UpdateEventArgs evu) =>
                {

                    var dice = new Dice();

                    bool stopRoll = false;


                    dice.pointer = "times";
                    dice.previousOperation = "";
                    string rolllList = "";

                    var update = evu.Update;
                    var message = update.Message;

                    if (message == null) return;
                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                    {
                        if (message.From.LastName != null)
                        {
                            userKey = message.From.LastName;
                        }
                        else
                        {
                            userKey = message.From.FirstName;
                        }

                        if (message.Text[0] == '/')
                        {
                            try
                            {
                                File.AppendAllText("log.txt", string.Format("{0} {1} {2} {3}", userKey, message.Text, DateTime.Now, Environment.NewLine));
                            }
                            catch (Exception)
                            { }
                        }

                        if (message.Text.Contains("@dbdroll_bot"))
                        {
                            var start = 0;
                            for (var i = 0; i < message.Text.Length && start == 0; i++)
                            {
                                if (message.Text[i] == '@')
                                    start = i;
                            }

                            message.Text = message.Text.Remove(start, 12);
                        }

                        if (message.Text.Length > 2 && message.Text[0] == '/' && message.Text[1] == 'r' && message.Text[2] == ' ')
                        {
                            for (var i = 3; i < message.Text.Length; i++)
                            {
                                double add = Char.GetNumericValue(message.Text[i]);
                                switch (message.Text[i])
                                {
                                    case 'd':
                                        {
                                            if (dice.mod != 0)
                                            {
                                                dice.count = dice.mod;
                                                dice.mod = 0;
                                            }
                                            dice.pointer = "max";
                                            if (dice.count == 0)
                                                dice.count++;
                                            toRoll = true;
                                            break;
                                        }
                                    case '+':
                                        {
                                            if (rolllList.Length == 0) rolllList = Roll(dice);
                                            if (toRoll) rolllList = Roll(dice, rolllList);
                                            if (dice.mod != 0)
                                            {
                                                rolllList = ModList(rolllList, dice);
                                            }
                                            resetDice(dice, "add", "+");
                                            break;
                                        }
                                    case '-':
                                        {
                                            if (rolllList.Length == 0) rolllList = Roll(dice);
                                            if (toRoll) rolllList = Roll(dice, rolllList);
                                            if (dice.mod != 0)
                                            {
                                                rolllList = ModList(rolllList, dice);
                                            }
                                            resetDice(dice, "sub", "-");
                                            break;
                                        }
                                    case '*':
                                        {
                                            if (rolllList.Length == 0) rolllList = Roll(dice);
                                            if (toRoll) rolllList = Roll(dice, rolllList);
                                            if (dice.mod != 0)
                                            {
                                                rolllList = ModList(rolllList, dice);
                                            }
                                            resetDice(dice, "mod", "*");
                                            break;
                                        }
                                    case '/':
                                        {
                                            if (rolllList.Length == 0) rolllList = Roll(dice);
                                            if (toRoll) rolllList = Roll(dice, rolllList);
                                            if (dice.mod != 0)
                                            {
                                                rolllList = ModList(rolllList, dice);
                                            }
                                            resetDice(dice, "div", "/");
                                            break;
                                        }
                                    default:
                                        {
                                            switch (dice.pointer)
                                            {

                                                case "times":
                                                    {
                                                        if (add < 0 || add > 10)
                                                        {
                                                            i = message.Text.Length;
                                                            stopRoll = true;
                                                            break;
                                                        }
                                                        dice.count = dice.count * 10 + add;
                                                        if (dice.count > 100000)
                                                        {
                                                            i = message.Text.Length;
                                                            stopRoll = true;
                                                        }
                                                        break;
                                                    }
                                                case "max":
                                                    {
                                                        if (add < 0 || add > 10)
                                                        {
                                                            i = message.Text.Length;
                                                            stopRoll = true;
                                                            break;
                                                        }
                                                        dice.max = dice.max * 10 + add;
                                                        if (dice.max > 100000)
                                                        {
                                                            i = message.Text.Length;
                                                            stopRoll = true;
                                                        }
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        if (add < 0 || add > 10)
                                                        {
                                                            i = message.Text.Length;
                                                            stopRoll = true;
                                                            break;
                                                        }
                                                        dice.mod = dice.mod * 10 + add;
                                                        if (dice.mod > 100000)
                                                        {
                                                            i = message.Text.Length;
                                                            stopRoll = true;
                                                        }
                                                        break;
                                                    }
                                            }

                                            break;
                                        }
                                }
                            }

                            if (!stopRoll)
                            {
                                if (dice.mod != 0) rolllList = ModList(rolllList, dice);
                                if (rolllList.Length == 0)
                                {
                                    rolllList = Roll(dice);

                                }
                                else
                                {
                                    if (dice.pointer == "max") rolllList = Roll(dice, rolllList);
                                }

                                if (rolllList.Length > 200)
                                {
                                    string listToSend = "";
                                    for (var i = 0; i < 200; i++)
                                        listToSend += rolllList[i];
                                    listToSend += "...";

                                    try
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id,
                                            listToSend + "; \nSum = " + (int)dice.totalRoll,
                                            replyToMessageId: message.MessageId);
                                    }
                                    catch (Exception ex)
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id, ex.Message,
                                            replyToMessageId: message.MessageId);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id,
                                            rolllList + "; \nSum = " + (int)dice.totalRoll,
                                            replyToMessageId: message.MessageId);
                                    }
                                    catch (Exception ex)
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id, ex.Message,
                                            replyToMessageId: message.MessageId);
                                    }
                                }
                            }
                            else
                            {
                                if (dice.max > 100000 || dice.count > 100000 || dice.mod > 100000)
                                {
                                    try
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id,
                                            "Please don't use numbers, bigger then 100000",
                                            replyToMessageId: message.MessageId);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id,
                                            "Invalid entry. please stick to /r XdY+Z format",
                                            replyToMessageId: message.MessageId);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }
                        else if (message.Text == "/statroll")
                        {
                            dice.count = 4;
                            dice.max = 6;
                            dice.previousOperation = "";

                            var stats = "";
                            var modSum = 0;
                            var statSum = 0;
                            var pointSum = 0;

                            for (var i = 0; i < 6; i++)
                            {
                                stats += i + 1 + " roll";
                                stats = Roll(dice, stats);
                                modSum += ((int)dice.totalRoll / 2 - 5);
                                statSum += (int)dice.totalRoll;
                                pointSum += PointBuy((int)dice.totalRoll);
                                stats += "Total - " + dice.totalRoll + " Mod =" + ((int)dice.totalRoll / 2 - 5) + " Point buy =" + PointBuy((int)dice.totalRoll) + "\n";
                                dice.totalRoll = 0;
                            }

                            stats += "Total modifier =" + modSum + "   Total points = " + statSum + "    Point Buy = " + pointSum;

                            try
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, stats,
                                    replyToMessageId: message.MessageId);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else if (message.Text == "/moneyinfo")
                        {
                            bool sent = false;
                            foreach (var entry in money)
                            {
                                if (entry.Key == userKey)
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id,
                                        userKey + "  " + entry.Value + "\n",
                                        replyToMessageId: message.MessageId);
                                    sent = true;
                                }
                            }

                            if (!sent)
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id,
                                    "You are not in system, please set your money first",
                                    replyToMessageId: message.MessageId);
                            }
                        }
                        else if (message.Text == "/exp all")
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, "Total exp list is:\n");
                            foreach (var entry in exp)
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, entry.Key + "  " + entry.Value);
                            }
                        }
                        else if (message.Text == "/moneyinfo all")
                        {
                            double sum = 0;
                            await Bot.SendTextMessageAsync(message.Chat.Id, "Total money:");
                            foreach (var entry in money)
                            {
                                sum += double.Parse(entry.Value);
                                await Bot.SendTextMessageAsync(message.Chat.Id, entry.Key + "  " + entry.Value + "\n");
                            }

                            await Bot.SendTextMessageAsync(message.Chat.Id, "Sum =" + sum);
                        }
                        else
                        if (message.Text[0] == '/' && message.Text[1] == 's' && message.Text[2] == 'e' &&
                            message.Text[3] == 't' && message.Text[4] == 'm' && message.Text[5] == 'o' &&
                            message.Text[6] == 'n' && message.Text[7] == 'e' && message.Text[8] == 'y' && message.Text.Length > 9)
                        {
                            if (!money.ContainsKey(userKey))
                            {
                                money.Add(userKey, "0");
                            }

                            toRoll = true; //for invalid entry
                            var dec = -1;
                            double newMoney = 0;

                            for (var i = 10; i < message.Text.Length; i++)
                            {
                                double add = Char.GetNumericValue(message.Text[i]);
                                if ((add < 0 || add > 10) && message.Text[i] != '.')
                                {
                                    i = message.Text.Length;
                                    toRoll = false;
                                }

                                if (toRoll)
                                {
                                    if (message.Text[i] == '.')
                                    {
                                        dec = 0;
                                    }

                                    if (dec < 0)
                                    {
                                        newMoney = newMoney * 10 + add;
                                    }
                                    else if (dec == 0)
                                    {
                                        dec++;
                                    }
                                    else
                                    {
                                        newMoney = newMoney + (add / (Math.Pow(10, dec)));
                                    }
                                }

                            }

                            if (toRoll)
                            {
                                money[userKey] = newMoney.ToString();
                                await Bot.SendTextMessageAsync(message.Chat.Id, "your money now =" +
                                    money[userKey] + "\n",
                                    replyToMessageId: message.MessageId);
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Invalid entry");
                            }

                            Save("money.txt");

                            toRoll = false;


                        }
                        else
                        if (message.Text[0] == '/' && message.Text[1] == 'a' && message.Text[2] == 'd' &&
                            message.Text[3] == 'd' && message.Text[4] == 'm' && message.Text[5] == 'o' &&
                            message.Text[6] == 'n' && message.Text[7] == 'e' && message.Text[8] == 'y' &&
                            message.Text.Length > 9)
                        {
                            if (!money.ContainsKey(userKey))
                            {
                                money.Add(userKey, "0");
                            }

                            toRoll = true; //for invalid entry
                            var dec = 0;
                            double newMoney = 0;
                            var start = 10;
                            if (message.Text[start] == '-')
                            {
                                start++;
                            }

                            for (var i = start; i < message.Text.Length; i++)
                            {
                                double add = Char.GetNumericValue(message.Text[i]);
                                if ((add < 0 || add > 10) && message.Text[i] != '.')
                                {
                                    i = message.Text.Length;
                                    toRoll = false;
                                }

                                if (toRoll)
                                {
                                    if (message.Text[i] == '.')
                                    {
                                        dec = 1;
                                    }
                                    else
                                    {
                                        if (dec == 0)
                                        {
                                            newMoney = newMoney * 10 + add;

                                        }
                                        else
                                        {
                                            newMoney = newMoney + (add / (Math.Pow(10, dec)));
                                            dec++;
                                        }
                                    }
                                }


                            }

                            if (toRoll)
                            {
                                if (start == 10)
                                {
                                    double.TryParse(money[userKey], out double oldMoney);
                                    money[userKey] = (oldMoney + newMoney).ToString();
                                }
                                else
                                {
                                    double.TryParse(money[userKey], out double oldMoney);
                                    money[userKey] = (oldMoney - newMoney).ToString();
                                }

                                await Bot.SendTextMessageAsync(message.Chat.Id, "your money now = " +
                                                                                money[userKey] + "\n",
                                    replyToMessageId: message.MessageId);

                                Save("money.txt");

                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Invalid entry");
                            }

                        }
                        else if (message.Text[0] == '/' && message.Text[1] == 'd' && message.Text[2] == 'm' && message.Text.Length > 3)
                        {
                            inputKey = "";
                            inputValue = "";
                            bool split = false;
                            bool set = false;

                            for (var i = 4; i < message.Text.Length; i++)
                            {
                                if (message.Text[i] == ' ')
                                {
                                    split = true;
                                }
                                else
                                {
                                    if (split)
                                    {
                                        inputValue += message.Text[i];
                                    }
                                    else
                                    {
                                        inputKey += message.Text[i];
                                    }
                                }
                            }

                            double.TryParse(inputValue, out double value);
                            if (inputKey != "set" && inputKey != "exp")
                            {
                                if (inputKey == "all")
                                {
                                    var keyHelper = new List<string>();
                                    foreach (var user in money)
                                    {
                                        keyHelper.Add(user.Key);
                                    }

                                    foreach (var tmp in keyHelper)
                                    {
                                        money[tmp] = (double.Parse(money[tmp]) + value).ToString();
                                    }

                                    await Bot.SendTextMessageAsync(message.Chat.Id, " New money list: ");
                                    foreach (var user in money)
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id,
                                            user.Key + "  " + user.Value + "\n");
                                    }

                                    Save("money.txt");

                                }
                                else if (money.ContainsKey(inputKey))
                                {
                                    money[inputKey] = (double.Parse(money[inputKey]) + value).ToString();

                                    await Bot.SendTextMessageAsync(message.Chat.Id, inputKey + "'s money now = " +
                                                                                    money[inputKey] + "\n",
                                        replyToMessageId: message.MessageId);

                                    Save("money.txt");
                                }
                                else
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id,
                                        "There is no player with such name, so he was created");
                                    money.Add(inputKey, inputValue);
                                    await Bot.SendTextMessageAsync(message.Chat.Id,
                                        "User " + inputKey + " added.\n His money=" + inputValue);
                                    inputKey = "";
                                    inputValue = "";

                                    Save("money.txt");
                                }
                            }
                            else if (inputKey != "exp")
                            {
                                var keyHelper = new List<string>();
                                double sum = 0;
                                foreach (var user in money)
                                {
                                    keyHelper.Add(user.Key);
                                }

                                foreach (var tmp in keyHelper)
                                {
                                    sum += value;
                                    money[tmp] = value.ToString();
                                }

                                await Bot.SendTextMessageAsync(message.Chat.Id, " New money list: ");
                                foreach (var user in money)
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id,
                                        user.Key + "  " + user.Value + "\n");
                                }
                                await Bot.SendTextMessageAsync(message.Chat.Id, " Total money: " + sum);

                                Save("money.txt");
                            }
                            else
                            {
                                var keyHelper = new List<string>();
                                double sum = 0;
                                foreach (var user in exp)
                                {
                                    keyHelper.Add(user.Key);
                                }

                                foreach (var tmp in keyHelper)
                                {
                                    sum += value;
                                    exp[tmp] = Convert.ToInt32(value);
                                }

                                await Bot.SendTextMessageAsync(message.Chat.Id, " New exp list: ");
                                foreach (var user in exp)
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id,
                                        user.Key + "  " + user.Value + "\n");
                                }

                                Save("exp.txt");
                            }

                        }
                        //exp
                        else if (message.Text[0] == '/' && message.Text[1] == 'e' && message.Text[2] == 'x' && message.Text[3] == 'p' && message.Text.Length > 4)
                        {
                            if (!exp.ContainsKey(userKey))
                            {
                                exp.Add(userKey, 0);
                            }

                            var newExp = 0;
                            toRoll = true;
                            var start = 5;
                            if (message.Text[5] == '-')
                            {
                                start = 6;
                            }

                            for (var i = start; i < message.Text.Length; i++)
                            {

                                double add = Char.GetNumericValue(message.Text[i]);
                                if ((add < 0 || add > 10) && message.Text[i] != '-')
                                {
                                    i = message.Text.Length;
                                    toRoll = false;
                                }

                                if (toRoll)
                                {
                                    newExp = newExp * 10 + Convert.ToInt32(add);
                                }

                            }

                            if (toRoll)
                            {
                                if (start == 6)
                                {
                                    newExp *= -1;
                                }
                                newExp += exp[userKey];
                                await Bot.SendTextMessageAsync(message.Chat.Id, " Exp changed, new exp = " + newExp);
                                exp[userKey] = newExp;
                                Save("exp.txt");
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Invalid entry");
                            }

                            toRoll = false;
                        }
                    }
                };

                Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message); // если ключ не подошел - пишем об этом в консоль отладки
            }
        }

        private int PointBuy(int points)
        {
            var total = 0;
            if (points <= 8)
            {
                total = points - 8;
            }
            else
            {
                if (points <= 14)
                    total = points - 8;
                else
                {
                    if (points <= 16)
                        total = (points - 14) * 2 + 6;
                    else
                    {
                        total = (points - 17) * 3 + 13;
                    }
                }
            }

            return total;
        }

        private string Roll(Dice dice)
        {
            string rolllList = " [ ";
            Random roll = new Random();

            for (var i = 0; i < dice.count; i++)
            {
                var thisRoll = roll.Next(1, (int)dice.max + 1);
                rolllList = rolllList + "(" + thisRoll + ")";
                dice.totalRoll += thisRoll;
                if (i < dice.count - 1) rolllList += " + ";
                else
                {
                    rolllList += " ] ";
                }
            }

            toRoll = false;
            return rolllList;
        }

        private string Roll(Dice dice, string oldList)
        {
            string rolllList = oldList + dice.previousOperation + " [";
            var currentSet = 0;
            var min = 0;

            for (var i = 0; i < dice.count; i++)
            {
                var thisRoll = roll.Next(1, (int)dice.max + 1);
                rolllList = rolllList + "(" + thisRoll + ")";
                currentSet += thisRoll;
                if (i < dice.count - 1) rolllList += " + ";
                else
                {
                    rolllList += " ] ";
                }

                if (dice.previousOperation == "")
                {
                    if (min == 0)
                    {
                        min = thisRoll;
                    }
                    else if (min > thisRoll)
                    {
                        min = thisRoll;
                    }
                }
            }

            switch (dice.previousOperation)
            {
                case "+":
                    {
                        dice.totalRoll += currentSet;
                        break;
                    }
                case "-":
                    {
                        dice.totalRoll -= currentSet;
                        break;
                    }
                case "*":
                    {
                        dice.totalRoll *= currentSet;
                        break;
                    }
                case "/":
                    {
                        dice.totalRoll /= currentSet;
                        break;
                    }
                default:
                    {
                        dice.totalRoll += currentSet;
                        break;
                    }
            }

            if (dice.previousOperation == "")
            {
                dice.totalRoll -= min;
            }

            return rolllList;
        }

        private string ModList(string oldList, Dice dice)
        {
            if (dice.pointer != "times" || dice.pointer != "max" && dice.mod != 0)
            {
                switch (dice.pointer)
                {
                    case "add":
                        {
                            dice.totalRoll += dice.mod;
                            return oldList + " + " + dice.mod;
                        }
                    case "sub":
                        {
                            dice.totalRoll -= dice.mod;
                            return oldList + " - " + dice.mod;
                        }
                    case "mod":
                        {
                            dice.totalRoll *= dice.mod;
                            return oldList + " * " + dice.mod;
                        }
                    case "div":
                        {
                            dice.totalRoll /= dice.mod;
                            return oldList + " / " + dice.mod;
                        }
                }
            }

            return oldList;
        }

        private void resetDice(Dice dice, string pointer, string previousOperation)
        {
            dice.pointer = pointer;
            dice.previousOperation = previousOperation;
            dice.count = 0;
            dice.max = 0;
            dice.mod = 0;
        }

        private double combiner(List<string> num, int dec)
        {
            double total = 0;
            int add;
            foreach (var entry in num)
            {
                if (Int32.TryParse(entry, out add))
                {
                    total = total * 10 + add;
                }
            }

            if (dec > 0)
                total /= (Math.Pow(10, dec));

            return total;
        }

        private void Save(string fileName)
        {
            File.WriteAllText(fileName, string.Empty);
            if (fileName == "money.txt")
            {
                using (StreamWriter file = new StreamWriter(fileName))
                {
                    foreach (var entry in money)
                        file.WriteLine("[{0} {1}]", entry.Key, entry.Value);
                }
            }
            else if (fileName == "exp.txt")
            {
                using (StreamWriter file = new StreamWriter(fileName))
                {
                    foreach (var entry in exp)
                        file.WriteLine("[{0} {1}]", entry.Key, entry.Value);
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            money.Clear();
            var input = File.ReadAllLines("money.txt");
            bool split = false;
            int dec = 0;
            string key = "";
            List<string> value = new List<string>();
            for (var j = 0; j < input.Length; j++)
            {
                string stroka = input[j];
                for (var i = 0; i < stroka.Length; i++)
                {
                    if (dec > 0)
                        dec++;

                    if (stroka[i] == ',')
                        dec++;

                    if (!split)
                    {
                        if (stroka[i] != ' ')
                        {
                            if (stroka[i] != '[' && stroka[i] != ']')
                                key += stroka[i];
                        }
                        else
                        {
                            split = true;
                        }
                    }
                    else
                    {
                        value.Add(stroka[i].ToString());
                    }


                }

                dec -= 2;

                money.Add(key.ToString(), combiner(value, dec).ToString());
                key = "";
                value.Clear();
                split = false;

                dec = 0;
            }

            input = File.ReadAllLines("exp.txt");
            for (var j = 0; j < input.Length; j++)
            {
                string stroka = input[j];
                for (var i = 0; i < stroka.Length; i++)
                {
                    if (!split)
                    {
                        if (stroka[i] != ' ')
                        {
                            if (stroka[i] != '[' && stroka[i] != ']')
                                key += stroka[i];
                        }
                        else
                        {
                            split = true;
                        }
                    }
                    else
                    {
                        value.Add(stroka[i].ToString());
                    }


                }


                exp.Add(key.ToString(), Convert.ToInt32(combiner(value, 0)));
                key = "";
                value.Clear();
                split = false;
            }

            var text = @txtKey.Text; // получаем содержимое текстового поля txtKey в переменную text
            if (text != "" && this.bw.IsBusy != true)
            {
                this.bw.RunWorkerAsync(text); // передаем эту переменную в виде аргумента методу bw_DoWork
                button1.Text = "Bot is working";
            }
        }
    }
}
