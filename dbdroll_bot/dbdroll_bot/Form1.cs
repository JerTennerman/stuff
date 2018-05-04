using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;

namespace dbdroll_bot
{
    public partial class Form1 : Form
    {
        BackgroundWorker bw;
        Random roll = new Random();
        bool toRoll = false;

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
                                pointSum += PointBuy((int) dice.totalRoll);
                                stats += "Total - " + dice.totalRoll + " Mod =" + ((int)dice.totalRoll / 2 - 5) +" Point buy =" + PointBuy((int)dice.totalRoll) + "\n";
                                dice.totalRoll = 0;
                            }

                            stats += "Total modifier =" + modSum + "   Total points = " + statSum +"    Point Buy = " + pointSum; 

                            try
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, stats,
                                    replyToMessageId: message.MessageId);
                            }
                            catch (Exception)
                            {
                            }
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
                        total = (points - 14)*2+6;
                    else
                    {
                        total = (points - 17) * 3 +13;
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

        private void button1_Click(object sender, EventArgs e)
        {
            var text = @txtKey.Text; // получаем содержимое текстового поля txtKey в переменную text
            if (text != "" && this.bw.IsBusy != true)
            {
                this.bw.RunWorkerAsync(text); // передаем эту переменную в виде аргумента методу bw_DoWork
                button1.Text = "Bot is working";
            }
        }
    }
}
