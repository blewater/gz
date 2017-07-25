(function () {
    'use strict';
    var ctrlId = 'helpCtrl';
    APP.controller(ctrlId, ['$scope', '$sce', '$timeout', ctrlFactory]);
    function ctrlFactory($scope, $sce, $timeout) {
        $scope.getHtml = function (text) {
            return $sce.trustAsHtml(text);
        };

        $scope.sections = [
            {
                name: 'My Account Section',
                items: [
			        { Q: "I have forgotten my username and password, what do I do?", A: "If you have forgotten your password, please use the 'Forgot Password?' option, follow the instructions and a new password will be issued to the e-mail address registered with your account. If you have forgotten your username, please contact help-at-greenzorro-dot-com." },
			        { Q: "Who do I contact if I have questions about my account?", A: "Please contact Customer Support by mail at help@greenzorro.com or directly via Live Chat." },
			        { Q: "What do I do if I no longer want to use my account?", A: "For closing your account please email Customer Support at help@greenzorro.com" },
			        { Q: "How do I change my password?", A: "You can change your password by accessing 'Change Password', under 'My Account' section, when logged in, and following the instructions on that page." },
			        { Q: "How do I change my registered email address?", A: "You can change your registered email address by accessing 'Change Email', under 'My Account' section, when logged in, and following the instructions on that page." },
			        { Q: "Is there any way I can check my account activity?", A: "You can check your account activity by accessing 'My Account' once you have logged in, or you can contact Customer Support for any information." },
			        { Q: "Is it possible to change my username?", A: "No, your registered username cannot be changed unfortunately." },
			        { Q: "Why is it that I cannot login to my account?", A: "Check if you are using the correct login details for your account, and that Caps Lock is turned off on your keyboard. You can also try to reset your password using the link 'Forgot Password?' next to the login fields. If you still cannot login please contact Customer Support." },
			        { Q: "Who can I consult if I think I have a problem with online gaming?", A: "Participating in online gaming is exciting, fun and potentially profitable for those that chose to play. However, we acknowledge that some people can develop a gaming addiction. If you feel that your gambling has gotten out of control, please visit our Responsible Gaming section found in the footer of the website and/or use the cool off and self-exclusion feature available from ‘My Account‘ when you are logged in." },
			        { Q: "How can I self-exclude?", A: "Self-exclusion is treated differently, depending under which jurisdiction you are playing. Please refer to Terms and conditions on how to self-exclude." },
			        { Q: "What happens when the self-exclusion period ends?", A: "This will differ from jurisdiction to jurisdiction, as self-exclusion is framed differently depending on which jurisdiction is applicable to you. Typically, when your self-exclusion ends, your account will be automatically reopened. In certain other jurisdictions, your account cannot be reopened, for your own protection, without positive action from you. Please read the Terms and Conditions for specific information on which self-exclusion rules are applicable to you." },
			        { Q: "Can I cancel it?", A: "This is jurisdiction specific. Typically, self-exclusion can be reduced or canceled, however there are some jurisdictions where it is not allowed to cancel or reduce self-exclusion. Please refer to the Terms and Conditions to see which self-exclusion rules are applicable to you." },
			        { Q: "I am not getting a response to my questions when I e-mail the Customer Support Team?", A: "We aim to reply to all customer support e-mails within a few hours. As some inquiries require a thorough investigation, response time may vary based on the nature of your e-mail. You will receive an e-mail confirmation from us when we have received your inquiry, to let you know we are working on an answer. If you find that you are not receiving the confirmation e-mail promptly after submitting your inquiry we would suggest that you have a look in your Junk Mail folder. We do not engage in the act of sending unsolicited e-mails however some aggressive mail filters may mistakenly identify our e-mails as spam and treat them as such." },
                ]
            },
            {
                name: 'Registration Section',
                items: [
			        { Q: "How do I open an account?", A: "Simply click the 'Join Now' button, available in the header of any www.greenzorro.com page to register. You will then be directed to the registration page. Please note that your username and your account currency cannot be changed at a later stage. Before submitting your details please make sure you declare that you are over 18 and agree to our 'Terms and Conditions'. You can immediately log into your account on any page using your username and password." },
			        { Q: "I'm having problems on the registration page. Now what?", A: "Please contact Customer Support Service at help@greenzorro.com to assist you with the registration." },
			        { Q: "Why do I need to open an account to play your games?", A: "We need you to open an account with us because by doing so you are agreeing to abide by our rules and regulations which include you are over the age of 18. By registering at www.greenzorro.com you are opening an account free of charge that enables you to use our products in fun mode and real money mode." },
			        { Q: "Why is my personal information required? Are my personal details safe with you?", A: "We need your personal details to confirm your identity, age, and address to be able to offer real money transactions and games to you. <span class=\"text-gz\"><b>greenzorro</b></span> uses the newest and most advanced data encryption techniques to safeguard your personal detail." },
			        { Q: "Do I need to make a deposit to be able to open an account at <span class=\"text-gz\"><b>greenzorro</b></span>?", A: "No, not at all. You can play for fun on our website and as much and as often as you like. In order to be able to use our products in real money mode, you need to make a deposit." },
			        { Q: "Can I have more than one account?", A: "No, only one account is permitted." },
			        { Q: "Is there any minimum age requirement to play at <span class=\"text-gz\"><b>greenzorro</b></span>?", A: "You must be at least 18 years of age to play on the <span class=\"text-gz\"><b>greenzorro</b></span> site." },
			        { Q: "Do you accept players from all around the world?", A: "We accept players from a vast list of countries but it is down to local legislation if the country permit gambling and therefore we prohibit player registrations in certain countries. All countries from which we allow registrations can be found in the drop down menu in the registration form." },
                ]
            },
            {
                name: 'Security Section',
                items: [
			        { Q: "Is your website licensed?", A: "Yes, our website is licensed. This is illustrated on the homepage of the website as well as in the Terms and Conditions, where information about the relevant license is provided." },
			        { Q: "How do I know my financial and personal information is secure?", A: "We utilize the latest in industry approved Secure Sockets Layer (SSL) encryption technology to ensure any information you send to us is kept safe, secure and fully encrypted. SSL encryption is the same technology banks use to protect data. This means, your financial data such as credit card information and your personal data such as name, address and phone number, all are fully protected." },
			        { Q: "How do I know my money is safe?", A: "All customer funds are held in a separate account with the Bank of Valletta (www.BOV.com). This way your funds are completely ring-fenced and isolated from company funds and accounts. That means your funds are safe and always readily available for you to withdraw." },
			        { Q: "Is my data shared or available to any third party?", A: "For information on how your data may be shared and under which circumstances, please refer to our Privacy Policy." },
                ]
            },
            {
                name: 'Financial Section',
                items: [
			        { Q: "How can I identify transactions to and from <span class=\"text-gz\"><b>greenzorro</b></span> on my credit card or bank statement?", A: "In most cases, deposits and withdrawals will be processed by EveryMatrix. If the payment is processed by a third party, EveryMatrix will not appear on the bank statement. If you do not recognize the company that appears on your bank/card statement, please contact support to confirm." },
			        { Q: "Do you accept deposits from my country?", A: "If you are playing from a restricted jurisdiction, we may not be able to accept deposits from you." },
			        { Q: "Why does my card deposit get declined or rejected?", A: "This can happen for a number of reasons, for example, the most common of which is incorrect entry of card data upon registration. Additionally, your bank may decline a transaction themselves due to insufficient funds on your card, in which case you should contact the bank directly. Also, some banks maintain a rule that does not allow you to make direct deposits to online gaming sites. Another reason may be that your transaction may have triggered our risk rule. Our support can be helpful in finding other reasons why the deposit did not succeed and provide a solution. You may want to contact your card provider to check, or try one of our many other deposit methods instead." },
			        { Q: "What is an IBAN, swift code and BIC?", A: "IBAN stands for International Bank Account Number. Which you can use when making or receiving international payments. It consists of up to 34 alphanumeric characters. You can usually find your IBAN on your bank statement or directly from your bank. A swift code is a code that is unique to your branch of the bank (also known as a Bank Identifier Code or BIC). This code usually consists of either 8 or 11 alphanumeric characters. If you are in any doubt, please contact your branch to confirm the correct code." },
			        { Q: "What is a CVV2/CVC2 code?", A: "CVV2/CVC2 stand for 'Card Verification Value/Code'. It is a code that is required as a security measure when making internet payments by credit card. The code consists of three digits and can be found on the back of your credit card." },
			        { Q: "Can I use my card to deposit into my friend's account?", A: "You cannot do this as this is deemed to be a third party transaction, which is not allowed." },
			        { Q: "Can I use more than one payment method?", A: "Yes, you can use several payment methods to fund your account." },
                ]
            },
            {
                name: 'Deposit Section',
                items: [
			        { Q: "How do I make a deposit to my account?", A: "Once you login to your account, click the deposit button, you can then select your preferred payment method and follow the on screen instruction to make a deposit." },
			        { Q: "How long does it take for my deposit to be credited to my gaming account?", A: "For most payment methods your deposit will be credited instantly. In the case of bank transfer this can take up to 7 days to be received from the bank, at which point we will credit your account." },
			        { Q: "Which deposit methods do you have available?", A: "Once logged in, please click deposit button and you will see all of the payment methods available to you." },
			        { Q: "Which credit/debit cards do you accept?", A: "We accept all Visa and MasterCard/maestro Credit and Debit cards as depositing methods, although there might be restrictions on withdrawals to MasterCard/Maestro card." },
			        { Q: "Will I be charged any fees when depositing into my account?", A: "Deposit fees will vary depending on your chosen method. Once you login to your account and click deposit button, you will see the fee displayed on the payment methods that are available to you, be aware your Bank may charge you a fee for their service." },
			        { Q: "What is the minimum/maximum amounts that I can deposit at a time?", A: "The minimum deposit amount is €10 or your currency equivalent and the maximum deposit amount vary according to payment methods you used. Please click on Deposit once you login to see the maximum deposit amount." },
			        { Q: "Is there a way for me to set daily, weekly and monthly deposit limits?", A: "You are able to set Daily, Weekly or Monthly Deposit Limits. These can be set within 'My Account' sub section 'Responsible Gaming'. Limits can be amended at any time in the same section." },
			        { Q: "How do I transfer money from my bank account?", A: "Please click 'Deposit' in 'My Account' and chose Bank transfers. Then follow the instructions on the screen." },
                ]
            },
            {
                name: 'Withdrawals Section',
                items: [
			        { Q: "How can I make a withdrawal from my website account?", A: "To make a withdrawal, login your account and click the withdrawal button, select the methods that are available to you and follow the onscreen instruction to request a withdrawal." },
			        { Q: "Are there any limits on how much I can withdraw?", A: "Once login, click the withdrawal button and you will see the limit you can withdraw for the method you choose." },
			        { Q: "Can I cancel a withdrawal request?", A: "If the withdrawal has not been processed yet, you have the option to cancel it by going to My Account-> pending withdrawals or you can contact Customer Service." },
			        { Q: "Can I get my winnings paid back to my credit/debit card?", A: "Yes you can as long as it is the same card that you used to make your original deposit and the card is able  to accept a  withdraw back onto it." },
			        { Q: "I used my credit/debit card to deposit, can I request a withdrawal by any other mean?", A: "Our policy is to return the original deposit amount back to the card that is used for deposit and extra winning can be returned via other method. If the card does not accept withdrawal, your winning can be paid out via bank transfer but we might need you to provide some documentation." },
			        { Q: "Why can’t I choose by which method I want to get paid?", A: "Due to anti money laundering laws, it is our policy to ensure the money is going back to the original source that made the deposit when possible." },
			        { Q: "Do you charge any withdrawal fees?", A: "Once login, please click the withdrawal button and the fee will be displayed next to the methods that are available to you for withdrawal." },
			        { Q: "How long do withdrawals take to process?", A: "Once login, please click the withdrawal button and the processing time will be displayed next to the methods that are available to you for withdrawal." },
			        { Q: "Why is it that a withdrawal to my card takes days while a deposit is immediate?", A: "We have a number of controls and checks that take place before any withdrawal is processed and withdrawals will be processed immediately once verifications are complete. These checks are part of our ongoing commitment to maintaining the security of our customers’ funds. Any other delays will be due to the restrictions imposed by the payment providers." },
			        { Q: "Can I make a withdrawal from my account and have it sent to someone else?", A: "No, this is deemed as a third party transaction and will not be permitted in any circumstance." },
			        { Q: "What is your withdrawal policy?", A: "Our policy is to ensure the money is going back to the original source that made the deposit when possible." },
                ]
            },
            {
                name: 'Technical Section',
                items: [
			        {
			            Q: "What are the system minimum requirements to be able to play at <span class=\"text-gz\"><b>greenzorro</b></span>?",
			            A: "<div>" +
                                "<div><b>Computers</b></div>" +
                                "<div>Windows - Intel Pentium processor (Pentium II or higher recommended) 64mb ram.</div>" +
                                "<div>Macintosh - Power Macintosh Power PC processor (G3 or higher recommended) 64mb ram.</div>" +
                                "<div>Adobe Flash 10 or greater.</div>" +
                                "<div><span class=\"text-gz\"><b>greenzorro</b></span> is compatible with most Windows Operating systems (Windows 2000, XP, Vista and Windows 7, Windows 8).</div>" +
                                "<div>Internet Explorer versions lower than IE7 are not supported.</div>" +
                           "</div>" +
                           "<br/>" +
                           "<div>" +
                                "<div><b>Mobile Devices</b></div>" +
                                "<div>All mobile devices that can display a webpage</div>" +
                                "<div>The downloadable application for mobile devices is only supported by Android and iOS (Apple devices)</div>" +
                           "</div>"
			        },
			        { Q: "Why is it that I cannot connect to or open a casino game?", A: "Please check your internet connection and that you have the latest version of Adobe Flash installed. If the issue persist, please contact Customer Support." },
			        { Q: "The website or games are responding slowly or load slowly, what can I do to speed it up?", A: "If you have been experiencing a slow connection, please note that having several browsers open, music programs running or downloading files can all slow your computer and/or internet connection. Your local internet service provider may also be temporarily suffering from low bandwidth. Sharing your internet connection within your household and in your local area can also slow your connection speed. Another possible solution may be to restart your computer or mobile device." },
                ]
            },
            {
                name: 'Bonus and Promotions Section',
                items: [
			        { Q: "Where do I find promotions available to me?", A: "You will always find available promotions by visiting the Promotions section. However, we may also publish promotions via SMS, notifications on mobile devices, newsletters, Facebook and Twitter. We strongly suggest that you accept SMS and e-mails from <span class=\"text-gz\"><b>greenzorro</b></span> (this is set in My Account/Profile), as some of our greatest offers are sent to you this way." },
			        { Q: "How do I get a deposit bonus?", A: "This is made very easy with www.greenzorro.com as we list all bonuses available to you when you make a deposit. If you can choose between more bonuses, simply select the one you prefer from the list!" },
			        { Q: "Can I surrender/forfeit a bonus?", A: "Yes, you can surrender a bonus before you start to play or even after you have placed bets with the bonus funds. However, if you surrender a bonus after betting with it, any won amount will be deducted from your real funds. To surrender a bonus, go to 'My account', then select Active Bonuses and Casino from left menu. Click Forfeit button found under your active bonus to surrender it." },
			        { Q: "Can I play any casino games with my bonus?", A: "Most casino bonuses are available for all games, while some bonuses might be limited to a specific brand, or game type. For instance only Net Entertainment games or only Video Slots. The same applies to sports bonuses - some bonuses might only be available for a specific sport or betting type. However, if any such restrictions, this is always clearly specified in the terms & conditions of the bonus. If in doubt, please contact Customer Support." },
			        { Q: "How do I check that I have met the wagering requirement for a bonus?", A: "You can check your progress in 'My account' - 'Active Bonuses'." },
			        { Q: "For what reasons can I be excluded from a promotion?", A: "Some of the promotions will be eligible only for customers from certain countries or that are using specific payment methods. There may also be restrictions based on previous misuse of our promotions. For any assistance, please contact Customer Support Team by mail at help@greenzorro.com or via Live Chat." },
                ]
            },
            //{
            //    name: 'Sports Section',
            //    items: [
			//        { Q: "Which software provider do you use?", A: "Our Sportsbook and Virtual Sports, desktop and mobile, is provided by EveryMatrix, an award winning sportsbook platform." },
			//        {
			//            Q: "What are the minimum and maximum stakes allowed?",
			//            A: "<div>Maximum stakes vary according to the event. If you try to place a bet greater than the maximum limit, you will receive a message telling you the maximum amount that you are permitted to bet on that selection. Below are the minimum stake limitations, depending on the currency used:</div>" +
            //                "<br/>" +
            //                "<table class=\"table table-striped\">" +
            //                    "<thead>" +
            //                        "<tr>" +
            //                            "<th>Min. Stake</th>" +
            //                            "<th>Currency</th>" +
            //                        "</tr>" +
            //                    "</thead>" +
            //                    "<tbody>" +
            //                        "<tr>" +
            //                            "<td>0.1</td>" +
            //                            "<td>GEL</td>" +
            //                        "</tr>" +
            //                        "<tr>" +
            //                            "<td>0.25</td>" +
            //                            "<td>CHF, EUR, GBP, LVL, USD</td>" +
            //                        "</tr>" +
            //                        "<tr>" +
            //                            "<td>0.5</td>" +
            //                            "<td>AUD, BGN, BRL, CAD, NZD, SGD, TRY</td>" +
            //                        "</tr>" +
            //                        "<tr>" +
            //                            "<td>1</td>" +
            //                            "<td>CNY, DKK, HKD, HRK, ILS, LTL, MXN, MYR, NOK, PLN, RON, SEK, ZAR</td>" +
            //                        "</tr>" +
            //                        "<tr>" +
            //                            "<td>5</td>" +
            //                            "<td>CZK</td>" +
            //                        "</tr>" +
            //                        "<tr>" +
            //                            "<td>10</td>" +
            //                            "<td>INR, JPY, PHP, RUB, THB, VEF</td>" +
            //                        "</tr>" +
            //                        "<tr>" +
            //                            "<td>50</td>" +
            //                            "<td>HUF</td>" +
            //                        "</tr>" +
            //                        "<tr>" +
            //                            "<td>500</td>" +
            //                            "<td>KRW</td>" +
            //                        "</tr>" +
            //                        "<tr>" +
            //                            "<td>1000</td>" +
            //                            "<td>IDR</td>" +
            //                        "</tr>" +
            //                    "<tbody>" +
            //                "</table>"
			//            },
			//        { Q: "What are single bets and multiple bets?", A: "Single bets involve just one selection; a multiple bet requires more than one. The more selections you link together in a multiple (e.g. all the home teams in a round of the Premier League), the more you can win – but of course it is much harder!" },
			//        { Q: "How many selections can a multiple bet contain?", A: "The maximum number of selections we allow in a multiple bet is 20." },
			//        { Q: "How does handicap betting work?", A: "For detailed explanations of handicap betting (1x2 with Handicap and Asian Handicaps), visit the Betting Explanation section here." },
			//        { Q: "What happens if a match is postponed?", A: "Check the rules for that sport. Some sports require the match to be played on the same day for bets to stand, bets in other sports may remain open until the match is completed or officially abandoned." },
			//        { Q: "Why has my winning bet not been paid yet?", A: "We require official results to be declared before markets can be settled. Sometimes online sources such as live score sites can be incorrect so settlement may be delayed until results can be confirmed." },
			//        { Q: "I’ve changed my mind, can I cancel my bet?", A: "No. Once you have confirmed your bet and it has been accepted by our server, bets cannot be cancelled." },
			//        { Q: "Where can I find a record of my bets?", A: "Your betting and transaction histories can be found under 'My Sportbetting Account' when logged into the website." },
			//        { Q: "Why are winnings calculated differently on UK odds?", A: "Winnings will always be calculated using European Odds. Please note that despite displaying the odds with only 2 decimals after the decimal separator, when winnings are calculated we use the exact value of the odds - which might have more than 2 decimals -, without rounding to 2 decimals only." },
			//        { Q: "Betting is becoming addictive for me, what can I do?", A: "Betting with us should be an enjoyable pastime for everyone, but for a small minority of people, it can become addictive and create problems. Information regarding gambling addiction can be found via Responsible Gambling." },
            //    ]
            //},
            {
                name: 'Casino Section',
                items: [
			        { Q: "How do I know that the casino games are fair?", A: "We only offer casino games from large, well-recognized and trusted casino software providers. All providers are certified, which means they have been methodically and scientifically tested by e.g. Technical Systems Testing (TST) - one of the world's most experienced and trusted gaming test labs, and part of the  Gaming Laboratorites International (GLI) group, or by eCOGRA, a London-based internationally accredited testing agency and player protection and standards organisation that provides an international framework for best operational practice requirements, with particular emphasis on fair and responsible gambling. At our website, you can be 100% confident that the Random Number Generator (RNG) that returns the outcome of a casino game round is completely random and unbiased, no matter which game you play. We would never offer games or brands that do not come with trusted certifications." },
			        { Q: "Which software providers do you use?", A: "We offer casino games from the most reputable casino game providers in the industry. You can browse through games from specific providers on the casino page." },
			        { Q: "Where can I find detailed instructions on how to play a game?", A: "'Game Rules' for each game can be found when you open the game." },
			        { Q: "What are the average payouts for each casino game?", A: "The theoretical payout varies from game to game and it is determined by the game providers when developing the game itself." },
			        { Q: "Can I try the games for free?", A: "Yes, you can play the majority of games for free if you chose the 'Play for Fun' mode." },
			        { Q: "Can I remove a bet after placing it on the table?", A: "Yes, before the game round has started it is possible to change or remove a bet." },
			        { Q: "What happens when I lose connection during a game round?", A: "The game is stored on the game server, simply log in and open the same game(s) you played when you are connected again, and continue where you left off. Whether you wait 5 minutes or 5 months, any game round will be waiting for you to complete it!" },
                ]
            }
        ];

        $scope.expand = function (parentIndex, index) {
            for (var i = 0; i < $scope.sections.length; i++)
                for (var j = 0; j < $scope.sections[i].items.length; j++)
                    if (i !== parentIndex || j !== index)
                        $scope.sections[i].items[j].expanded = false;
            $scope.sections[parentIndex].items[index].expanded = !$scope.sections[parentIndex].items[index].expanded;
        };
    }
})();