using System;
using System.IO;
using System.Diagnostics;


namespace Kiosk_Group_Project {//start namespace
    class Program {//start class
        struct Bank {
            public string name; //bill or coin name
            public decimal worth; //how much specified bill or coin is worth
            public int stored; // how many of each is stored in the bank (upon start up)
            public int checking; //blank to become a copy or stored later to check against
        }//end struct
        struct Cards {
            public string vendor;//name of vendor (EX: Visa)
            public int start_digit; //digit vender card starts with
            public int max_digits; //max digit amount vender card has
        }//end struct

        static Bank[] money; //global call for kiosk bank information
        static Cards[] card; //global call for card information
        static void Main(string[] args) {//start main function
            #region VARIABLES
            //variables
            bool cashback = true;
            bool on = true;
            bool real = true;
            bool correct_change = true;
            bool paid = false;
            bool cancel_transaction = false;
            bool partialpaid = false;
            bool declined = false;
            bool valid_check = true;
            decimal total_cost = 0;
            decimal new_item_cost = 0;
            decimal cash_payment = 0;
            decimal total_payment = 0;
            decimal total_cash_payment = 0;
            decimal remaining_payment = 0;
            decimal change_due = 0;
            decimal cardpayment = 0;
            decimal total_card_payment = 0;
            decimal final_change_due = 0;
            int count = 0;
            int transaction_number = 0;
            int[] converted_card;
            string cashcard = "";
            string answer;
            string card_number = "";
            string vendor_name = "";
            string[] money_request;
            #endregion

            #region BOOT UP ARRAYS
            //ARRAYS FOR BOOTUP - CASH
            string[] names = { "One Hundred Dollar Bill", "Fifty Dollar Bill", "Twenty Dollar Bill", "Ten Dollar Bill", "Five Dollar Bill", "Two Dollar Bill", "One Dollar Bill", "One Dollar Coin", "Half Dollar", "Quarter", "Dime", "Nickel", "Penny" };
            decimal[] currencies = { 100.00m, 50.00m, 20.00m, 10.00m, 5.00m, 2.00m, 1.00m, 1.00m, 0.50m, 0.25m, 0.10m, 0.05m, 0.01m };
            int[] stored_count = { 5, 10, 20, 20, 50, 50, 100, 100, 200, 250, 500, 500, 0 };
            money = new Bank[names.Length];

            //ARRAYS FOR BOOTUP - CARDS
            string[] vendors = { "American Express Card", "Visa Card", "Master Card", "Discover Card" };
            char[] startdigit = { '3', '4', '5', '6' };
            int[] maxdigit = { 15, 16, 16, 16 };
            card = new Cards[vendors.Length];
            #endregion

            #region BOOTUP LOOP FOR DATA
            //SET UP FOR BOOTUP - CASH
            while(count < 13) {
                //store preloaded information into kiosk
                money[count].name = names[count];
                money[count].worth = currencies[count];
                money[count].stored = stored_count[count];
                count++;
            }//end while
            //reset count
            count = 0;
            //SET UP FOR BOOTUP - CARD
            while(count < card.Length) {
                card[count].vendor = vendors[count];
                card[count].start_digit = startdigit[count];
                card[count].max_digits = maxdigit[count];
                count++;
            }//end while
            #endregion

            //infinite loop while power is turned on
            while(on) {
                //RESET COUNT TO ONE AND LOOP UNTIL USER DOESN'T ADD AN ITEM PRICE
                count = 1;
                transaction_number++;

                #region INTRO
                Console.WriteLine("!--- WELCOME TO NOHOMOSAPIENS SELF CHECKOUT KIOSK ---!");
                Console.WriteLine("--- You may begin scanning your items ---\n");
                #endregion

                #region SCAN ITEMS
                do {//loop scan item until blank
                    do {//verification loop to make sure scan is greater than 0
                        new_item_cost = PromptDecimal($"--- Scan Item {count}:");
                    } while(new_item_cost < 0);

                    if(new_item_cost != 0) {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        //output "sound effect"
                        Console.WriteLine("\n**BOOP BEEP**");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("!- Place item in bagging area -!\n");
                        Console.ResetColor();
                        total_cost += new_item_cost; //add to total cost
                        count++;
                    }//end if                
                } while(new_item_cost != 0);

                //output cost
                Console.WriteLine($"\n--- Scanned {count - 1} items ---");
                Console.WriteLine($"--- Total Cost: ${total_cost} ---\n");
                #endregion

                #region CASH OR CARD PROMPT
                do {
                    do {
                        if(cashback == false) {
                            Console.WriteLine("!-- Kiosk does NOT have enough cash for cash back --!");
                        }//end if

                        //prompt for cash or card
                        if(cashcard == "") {
                            do {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                cashcard = PromptString("--- Would you like to pay with cash or card?");
                                Console.ResetColor();
                                cashcard = cashcard.ToLower();
                                if(cashcard != "cash" && cashcard != "card") {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"!-- Sorry, {cashcard} is not a valid function --!");
                                    Console.ResetColor();
                                }
                            } while(cashcard != "cash" && cashcard != "card");//loop if not cash or card

                        }//end if
                        #endregion
                        #region CASH PAYMENT
                        //start loops for user payments - cash
                        if(cashcard == "cash") {
                            do {
                                do {
                                    cash_payment = PromptDecimal("Please insert cash payment now:");
                                    real = UserPaymentCash(cash_payment);
                                } while(real != true);//loop if false, checks if real dollar bill or coin

                                //add payment to total payment
                                total_payment += cash_payment;
                                remaining_payment = total_cost - total_payment;
                                if(total_payment < total_cost) {
                                    Console.WriteLine($"\n--- Unpaid balance: ${remaining_payment} ---\n");
                                }//end if                
                            } while(total_cost > total_payment);//loop again if total_payment is less than total_cost

                            //set total_cash_payment = total_payment
                            total_cash_payment = total_payment;

                            //output change due
                            change_due = (remaining_payment * -1);
                            Console.WriteLine($"\n--- Change due: ${change_due} ---");
                            paid = true;
                            #endregion
                            #region CARD PAYMENT
                            #region CARD SWIPE AND VALIDATION CHECK
                        } else if(cashcard == "card") {//start loops for user payments - card
                            do {//validation loop for card number - loop again if < 15 or > 16
                                Console.WriteLine("\n--- Swipe or Insert card now ---");
                                card_number = PromptString("--- Card Number:");
                                if(card_number.Length < 15 || card_number.Length > 16) {//invalid card number - try again
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("\n!-- That is not a valid card number --!");
                                    Console.ResetColor();
                                }//end if
                            } while(card_number.Length < 15 || card_number.Length > 16);

                            if(cashback && !partialpaid) {
                                do {//validation loop for yes or no - cash back
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    answer = PromptString("\n--- Would you like cash back?");
                                    Console.ResetColor();
                                    answer = YesNoFunction(answer);
                                    if(answer != "yes" && answer != "no") {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"\n!-- Sorry, {answer} is not a valid function --!\n");
                                        Console.ResetColor();
                                    }//end if
                                } while(answer != "yes" && answer != "no");

                                if(answer == "yes") {
                                    do {//loop if $ < 0 - won't accept negative numbers
                                        Console.WriteLine("");
                                        change_due = PromptDecimal("--- Input Cash Back amount:");
                                        if(change_due < 0) {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"!-- Sorry, {change_due} is not a valid input --!");
                                            Console.ResetColor();
                                        }//end if
                                        total_cost += change_due;
                                        Console.WriteLine($"--- New total cost: ${total_cost} ---");
                                    } while(change_due < 0);
                                }//end if
                            } //end if

                            //convert card to int array
                            converted_card = CardNumberConvertFunction(card_number);

                            //display card vendor
                            vendor_name = CardVendorFunction(card, converted_card[0]);

                            //check if valid card
                            valid_check = ValidCardFunction(card, converted_card);
                            #endregion

                            //only if card is valid
                            if(valid_check) {
                                //check if bank has enough money
                                money_request = MoneyRequest(card_number, total_cost);
                                //USE BELOW FOR TESTING ONLY
                                //money_request[1] = "5.00";
                                
                                if(money_request[1] == "declined") {//string [1] returns "declined" give the card declined error
                                    paid = false;
                                    declined = true;
                                    total_cost -= change_due;
                                    cancel_transaction = BankDeclinedCard();
                                    cashcard = "";
                                } else {
                                    cardpayment = Convert.ToDecimal(money_request[1]);
                                    remaining_payment = total_cost - cardpayment;
                                    paid = BankPaidFunction(remaining_payment);
                                }//end if

                                //store cardpayment into total_card_payment
                                total_card_payment += cardpayment;

                                //this starts the payment accepted phase
                                if(!paid && !declined) {//then payment wasn't finished
                                    cancel_transaction = PartialPayOrCancel(remaining_payment, cardpayment);
                                    if(!cancel_transaction) {
                                        total_cost -= cardpayment;
                                        Console.WriteLine($"\n--- Remaining cost ${total_cost} ---\n");

                                        cashcard = PartialPay(cardpayment, remaining_payment);
                                        partialpaid = true;
                                    }//end if
                                }//end if
                            } else {//card invalid
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\n!-- That is an invalid card number --!\n");
                                Console.ResetColor();
                                answer = PromptString("--- Pay a different way?");
                                Console.WriteLine("");
                                answer = YesNoFunction(answer);
                                if(answer == "no") {
                                    cancel_transaction = true;
                                } else {
                                    cashcard = "";
                                    paid = false;
                                }//end if
                            }//end if
                        }//end if
                    } while(!paid && !cancel_transaction);
                    #endregion

                    #region CHANGE DUE
                    if(paid && !cancel_transaction && cashback) {
                        //start loop to check if bank has enough money
                        if(change_due == 0) {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("--- * Transaction complete * ---\n");
                            Console.ResetColor();
                        } else {
                            correct_change = CheckChange(change_due);
                        }//end if

                        //change return if true || refund money if false
                        if(change_due > 0) {
                            if(correct_change) {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("--- Dispensing change ---\n");
                                Console.ResetColor();
                                final_change_due = MoneyReturn(change_due);
                            } else {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\n!!! Kiosk does not have enough money to return change !!!\n");
                                if(cashcard == "cash") {
                                    Console.WriteLine("--- Refunding payment now");
                                    total_cash_payment = 0;
                                    final_change_due = 0;
                                    Console.ResetColor();
                                    MoneyReturn(total_payment);

                                    do {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        answer = PromptString("\n--- Would you like to pay with a card instead?");
                                        Console.ResetColor();
                                        answer = YesNoFunction(answer);
                                    } while(answer != "yes" && answer != "no");

                                    if(answer == "yes") {
                                        //send user back up to pay with card
                                        cashcard = "card";
                                        paid = false;
                                    } else {//if no then cancel transaction and get user out
                                        cancel_transaction = true;
                                    }//end if
                                }//end if 
                                Console.ResetColor();
                                cashback = false;

                            }//end if
                        }//end if
                    }//end if

                } while(!paid && !cancel_transaction);
                #endregion
                if(cancel_transaction == true) {
                    final_change_due = 0;
                    total_cash_payment = 0;
                    total_card_payment = 0;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("--- Transaction Cancelled ---");
                    Console.ResetColor();
                }//end if

                //set No Card if user paid cash
                if(cashcard == "cash") {
                    vendor_name = "No Card";
                }//end if

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("--- HAVE A NICE DAY ---");
                Console.ResetColor();                

                #region LOGGING
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "C:\\Users\\MCA\\source\\repos\\ChangeBot Transaction Logging Package\\ChangeBot Transaction Logging Package\\bin\\Release\\ChangeBot Transaction Logging Package.exe";
                startInfo.Arguments = $"{transaction_number} ${total_cash_payment} {vendor_name} ${total_card_payment} ${final_change_due}";
                Process.Start(startInfo);
                #endregion

                #region EXIT OLD RESET FOR NEW
                //reset for new customer
                final_change_due = 0;
                total_cash_payment = 0;
                total_card_payment = 0;
                change_due = 0;
                valid_check = true;
                total_cost = 0;
                total_payment = 0;
                cashcard = "";
                cashback = true;
                cancel_transaction = false;
                partialpaid = false;
                Console.WriteLine("\n\n\n\n");
                #endregion
            }//end while - ends when power off
        }//end main
        #region PARTIAL PAY FUNCTION
        static string PartialPay(decimal cardpay, decimal remaining) {
            string answer = "";
            Console.WriteLine($"--- ${cardpay} has been removed from the card ---\n");
            do {
                Console.ForegroundColor = ConsoleColor.Yellow;
                answer = PromptString($"--- How would you like to pay the remaining ${remaining}?");
                Console.ResetColor();
                Console.WriteLine("");
                answer = answer.ToLower();
                if(answer != "card" && answer != "cash") {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"!-- {answer} is an unacceptable response (Cash or Card)--!");
                    Console.ResetColor();
                }
            } while(answer != "card" && answer != "cash");

            return answer;
        }//end function
        #endregion

        #region PARTIAL PAY OR CANCEL FUNCTION
        static bool PartialPayOrCancel(decimal pay, decimal cardpay) {
            string answer = "";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"!-- Insufficent funds to finish transaction --!\n");
            Console.ResetColor();
            Console.WriteLine($"--- Remaining balance due will be ${pay} if you proceed ---\n");
            do {
                Console.ForegroundColor = ConsoleColor.Yellow;
                answer = PromptString($"--- Would you like to charge ${cardpay} to this card and pay another way?");
                Console.ResetColor();
                answer = YesNoFunction(answer);
            } while (answer != "yes" && answer != "no");
            if(answer == "yes") {
                return false;
            } else {
                return true;
            }
        }//end function
        #endregion

        #region BANK PAID FUNCTION
        static bool BankPaidFunction(decimal remaining) {
            if(remaining == 0) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("--- Payment Accepted ---\n");
                Console.ResetColor();
                return true;
            } else {
                return false;
            }
        }//end function
        #endregion

        #region BANK DECLINED FUCNTION
        static bool BankDeclinedCard() {
            string answer = "";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("!-- Card Declined --!\n");
            do {
                Console.ForegroundColor = ConsoleColor.Yellow;
                answer = PromptString("--- Pay a different way?");
                Console.ResetColor();
                answer = YesNoFunction(answer);
            } while (answer != "yes" && answer != "no");
            if(answer == "no") {
                return true;
            } else {
                return false;
            }//end if
        }//end function
        #endregion

        #region CARD VALIDATION FUNCTION
        static bool ValidCardFunction(Cards[] cards, int[] nums) {
            //variables
            int total = 0;
            int check_num = 0;
            int count = nums.Length - 1;
            int[] reversed = new int[nums.Length];
            bool odd = true;
            int modulus = 0;

            //loop to reverse card numbers
            for(int index = 0; index < nums.Length; index++) {
                reversed[index] = nums[count];
                count--;
            }//end for

            //set check_num = reversed[0]
            check_num = reversed[0];

            //loop to multiply odd by 2 and add all numbers to total
            for(int index = 1; index < nums.Length; index++) {
                if(odd) {//then
                    reversed[index] *= 2;
                    if(reversed[index] > 9) {//then
                        reversed[index] -= 9;
                    }//end if
                    total += reversed[index];
                    odd = false;
                } else {//then
                    total += reversed[index];
                    odd = true;
                }//end if
            }//end for

            modulus = (10 - (total % 10));

            if(10 - total % 10 == 10) {
                modulus = 0;
            }

            //check if total mod 10 equals the check number
            if(modulus == check_num) {
                return true;
            } else {
                return false;
            }//end if
        }//end function

        #endregion

        #region CONVERT CARD NUMBER TO INT ARRAY
        static int[] CardNumberConvertFunction(string card_number) {
            //set card number as an array
            int count = 0;
            int[] numbers = new int[card_number.Length];

            foreach(char num in card_number) {
                numbers[count] = Convert.ToInt32(num - 48);
                count++;
            }//end foreach
            return numbers;
        }//end function
        #endregion

        #region DISPLAY CARD VENDOR FUNCTION
        static string CardVendorFunction(Cards[] card, int number) {
            string vendor = "";

            for(int index = 0; index < card.Length; index++) {
                if((number + 48) == card[index].start_digit) {
                    Console.WriteLine($"\n--- Scanning your {card[index].vendor} now ---");
                    vendor = card[index].vendor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("!-- Please Do Not Remove Your Card --!\n");
                    Console.ResetColor();
                }//end if
            }//end for
            return vendor;
        }//end function
        #endregion

        #region MONEY RETURN
        static decimal MoneyReturn(decimal refund) {
            decimal total_returned = 0;

            //loop through bank to remove money
            for(int index = 0; index < money.Length; index++) {
                if(refund >= money[index].worth && money[index].stored > 0) {
                    refund -= money[index].worth;
                    money[index].stored--;
                    Console.WriteLine($"--- ${money[index].worth} Dispensed ---");
                    total_returned+= money[index].worth;
                    index = -1;
                }//end if
            }//end for
            Console.WriteLine("");
            return total_returned;
        }//end function
        #endregion

        #region CHECK CHANGE
        static bool CheckChange(decimal check_change) {//checks if bank has enough change stored
            //set checking to what is stored
            for(int index = 0; index < money.Length; index++) {
                money[index].checking = money[index].stored;
            }//end for

            //loop through bank to check all change
            for(int index = 0; index < money.Length; index++) {
                if(check_change >= money[index].worth && money[index].checking > 0) {//then subtract that bill from check_change and repeat
                    check_change -= money[index].worth;
                    money[index].checking--;
                    index = -1;
                }//end if
                if(check_change == 0) {//set true if check_change is 0
                    return true;
                }//end if
            }//end for
            return false;
        }//end function
        #endregion

        #region USER PAYMENT CASH
        static bool UserPaymentCash(decimal pay) {

            for(int index = 0; index < money.Length; index++) {//check through money.currency to make sure user input a matching payment
                if(pay == money[index].worth) {//if finds a match set real to true                    
                    money[index].stored++;
                    return true;
                }//end if
            }//end for
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"!-- ${pay} is not an acceptable payment --!");
            Console.ResetColor();
            return false;
        }//end function
        #endregion

        #region MONEY REQUEST
        //TO SIMULATE A BANK SENDING BACK A REQUEST FOR FUNDS
        static string[] MoneyRequest(string account_number, decimal amount) {
            Random rnd = new Random();
            //50% CHANCE TRANSACTION PASSES OR FAILS
            bool pass = rnd.Next(100) < 50;
            //50% CHANCE THAT A FAILED TRANSACTION IS DECLINED
            bool declined = rnd.Next(100) < 50;

            if(pass) {
                return new string[] { account_number, amount.ToString() };
            } else {
                if(!declined) {
                    return new string[] { account_number, (amount / rnd.Next(2, 6)).ToString() };
                } else {
                    return new string[] { account_number, "declined" };
                }//end if
            }//end if
        }//end function
        #endregion        

        #region Y to Yes | N to No
        static string YesNoFunction(string letter) {
            letter.ToLower();
            if(letter == "y") {
                letter = "yes";
            } else if(letter == "n") {
                letter = "no";
            } //end if
            return letter;
        }//end function
        #endregion

        #region PREBUILT FUNCTIONS
        static int PromptInt(string message) {//prompts for an int
            Console.Write(message + " ");
            return int.Parse(Console.ReadLine());
        }//end function

        static double PromptDouble(string message) {//prompts for a double
            Console.Write(message + " ");
            return double.Parse(Console.ReadLine());
        }//end function
        static decimal PromptDecimal(string message) {//prompts for a decimal
            Console.Write(message + " $");
            string read = Console.ReadLine();
            if(read == "") {
                return 0;
            } else {
                return decimal.Parse(read);
            }
        }//end function

        static string PromptString(string message) {//prompts for a string
            Console.Write(message + " ");
            return Console.ReadLine();
        }//end funciton

        static bool PromptBool(string message) {//prompts for a bool (may need fixing and/or changing of commands to work)
            Console.Write(message + " ");
            return bool.Parse(Console.ReadLine());
        }//end function

        static char PromptChar(string message) {//prompts for a char
            string check;

            //send message for char and loop if length ! = 1
            do {
                Console.Write(message + " ");
                check = Console.ReadLine();
            } while(check.Length != 1);
            return Convert.ToChar(check);//will only read first input
        }//end function

        static bool StringContains(string word, char letter) {//checks if word contains letter
            bool contains = false;

            //check if word[] contains letter
            for(int index = 0; index < word.Length; index++) {
                if(word[index] == letter) {//then 
                    contains = true;
                }//end if
            }//end for

            return contains;
        }//end function

        static string PadLeft(string word, char letter, int num) {
            string new_letter = Convert.ToString(letter);
            string new_word = "";

            //loop to make letter added to itself the amount in num
            for(int index = 0; index < num; index++) {
                new_word += new_letter;
            }//end for

            //add first word to padded word
            new_word = new_word + word;

            return new_word;
        }//end function

        static string StringRemove(string word, char letter) {
            string new_word = "";

            //loop to find if word[num] does not equal letter
            for(int index = 0; index < word.Length; index++) {
                if(word[index] != letter) {//then add to new_word
                    new_word += word[index];
                }//end if
            }//end for
            return new_word;
        }//end function

        static string StringIntersection(string s1, string s2) {
            string return_string = "";

            //LOOP TO CHECK INTERSECTIONS AND UNIQUE LETTERS
            foreach(char letter in s1) {
                if(in_string(letter, s2) && !in_string(letter, return_string)) {
                    return_string += letter;
                }//end if
            }//end foreach

            //LOCAL HELPER FUNCTION
            bool in_string(char character, string word) {
                foreach(char letter in word) {
                    if(letter == character) {
                        return true;
                    }//end if
                }//end foreach
                return false;
            }//end internal function

            return return_string;
        }//end function

        static string StringUnion(string s1, string s2) {
            string return_string = "";

            //LOOP TO CHECK UNIQUE LETTERS AGAINST s1 (it's self) and return_string
            foreach(char letter in s1) {
                if(in_string(letter, s1) && !in_string(letter, return_string)) {
                    return_string += letter;
                }//end if
            }//end foreach

            //LOOP TO CHECK UNIQUE LETTERS AGAINST s1 AND return_string
            foreach(char letter in s2) {
                if(!in_string(letter, s1) && !in_string(letter, return_string)) {
                    return_string += letter;
                }//end if
            }//end foreach

            //LOCAL HELPER FUNCTION
            bool in_string(char character, string word) {
                foreach(char letter in word) {
                    if(letter == character) {
                        return true;
                    }//end if
                }//end foreach
                return false;
            }//end internal function

            return return_string;
        }//end function

        static bool StringIsNumeric(string input) {
            //loop through each letter and check if < 48 or > 57 (ascii table)
            foreach(char letter in input) {
                if(letter < 48 || letter > 57) {
                    if(letter != 46) {//if not . then
                        return false;
                    }//end if                        
                }//end if                
            }//end foreach
            return true;
        }//end funcion

        static bool StringContains(string s1, string s2) {
            string check_string = "";

            //LOOP TO CHECK LETTERS
            foreach(char letter in s1) {
                if(in_string(letter, s2)) {
                    check_string += letter;
                }//end if
            }//end foreach

            //if check_string matches s2
            if(check_string == s2) {//then
                return true;
            } else {//then
                return false;
            }//end if


            //LOCAL HELPER FUNCTION
            bool in_string(char character, string word) {
                foreach(char letter in word) {
                    if(letter == character) {
                        return true;
                    }//end if
                }//end foreach
                return false;
            }//end internal function
        }//end function

        static string[] StringSplit(string s1, char c1) {
            string word_stored = "";
            int count = 1;

            foreach(char letter in s1) {
                if(c1 == letter) {
                    count++;
                }//end if                
            }//end foreach            

            string[] string_array = new string[count];

            //reset count
            count = -1;

            foreach(char letter in s1) {
                if(in_string(letter, s1)) {
                    word_stored += letter;
                } else {
                    count++;
                    string_array[count] = word_stored;
                    word_stored = "";
                }//end if
            }//end for each

            bool in_string(char character, string word) {
                if(character != c1) {
                    return true;
                }//end if
                return false;
            }//end internal function

            //for final stored string
            if(!s1.EndsWith(c1)) {
                count++;
                string_array[count] = word_stored;
            }//end if


            return string_array;
        }//end function

        static int[] BubbleSort(int[] data) {
            bool sorting = true;
            int stored_num;
            int count = 0;

            //loop if had to sort
            do {
                //sets everything to start check
                count = 0;
                sorting = false;

                while(count < data.Length && count + 1 != data.Length) {//this loops the entire contents unless it eaches the end
                    if(data[count] > data[count + 1]) {//if [num1] > [num2] store num1 then swap
                        stored_num = data[count];
                        data[count] = data[count + 1];
                        data[count + 1] = stored_num;
                        sorting = true;
                    }//end if
                    count++;
                }//end while    

            } while(sorting);//if had to sort loop again

            return data;
        }//end function

        static int BinarySearch(int[] data, int search) {
            int left = 0;
            int right = data.Length;
            int mid = (left + right) / 2;
            int low_num = data[left];
            int high_num = data[right - 1];

            //call bubble sort to put array in order
            BubbleSort(data);

            //if higher than highest or lower than lowest number skip search
            if(search > high_num || search < low_num) {
                return -1;
            }//end if
            //loop until left > right            
            while(left <= right) {
                if(search == data[mid]) {//then output mid
                    return mid;
                } else if(search < data[mid]) {//then calculate right and change mid accordingly
                    right = mid - 1;
                    mid = (left + right) / 2;
                } else if(search > data[mid]) {//then calculate left and change mid accordingly
                    left = mid + 1;
                    mid = (left + right) / 2;
                }//end for
            }//end while

            //if not found return -1            
            return -1;
        }//end function

        #endregion
    }//end class

}//end namespace
