(function () {
    'use strict';
    var ctrlId = 'faqCtrl';
    APP.controller(ctrlId, ['$scope', '$sce', '$timeout', ctrlFactory]);
    function ctrlFactory($scope, $sce, $timeout) {
        $scope.getHtml = function(text) {
			return $sce.trustAsHtml(text);
        };

        $scope.sections = [
            {
                name: 'About greenzorro',
                items: [
			        { Q: "What is <span class=\"text-gz\"><b>greenzorro</b></span>?", A: "<span class=\"text-gz\"><b>greenzorro</b></span> is an online gambling operator that combines gambling with investing, offering a unique value proposition to players. <span class=\"text-gz\"><b>greenzorro</b></span> allows players to invest all losses into liquid investments so that they can generate investment returns and recoup all their losses. Players can choose their own portfolio and exit at any time they want." },
			        { Q: "How is <span class=\"text-gz\"><b>greenzorro</b></span> similar to other online casinos?", A: "<div>Variety: <span class=\"text-gz\"><b>greenzorro</b></span> offers all popular casino games.</div><div>Winnings: At <span class=\"text-gz\"><b>greenzorro</b></span>, winning players can enjoy their winnings and can cash out or continue playing</div>" },
			        { Q: "What is the difference between <span class=\"text-gz\"><b>greenzorro</b></span> and other online casinos?", A: "<span class=\"text-gz\"><b>greenzorro</b></span> does <b>not</b> make any profit from gambling activities. Instead of pocketing all of player's losses, <span class=\"text-gz\"><b>greenzorro</b></span> allows you to invest any losses you have incurred, in a variety of investment products." },
			        { Q: "How does <span class=\"text-gz\"><b>greenzorro</b></span> estimate each player's losses?", A: "Losses are estimated daily for each player account individually, by summing up wins, and subtracting the losses. If this net balance is positive, then the player has won more money on the <span class=\"text-gz\"><b>greenzorro</b></span> platform and no action is required. If the net balance is negative, it means that the player has lost money and this amount is transferred over to the player's investment account, after subtracting operating costs." },
			        { Q: "How does <span class=\"text-gz\"><b>greenzorro</b></span> make money?", A: "<span class=\"text-gz\"><b>greenzorro</b></span> does not make any profit from Gambling activities. <span class=\"text-gz\"><b>greenzorro</b></span> generates income ONLY from investment activities where it charges industry standard fees. With this approach, <span class=\"text-gz\"><b>greenzorro</b></span> increases the overall value of players' investments and makes money ONLY on players' additional investment returns.<br><br><span class=\"text-gz\"><b>greenzorro</b></span> is backed by investors who recognize the need to create a great gaming platform that offers substantially more value to gamblers than existing offerings. Our investors are not focused on short term profits and this gives us adequate time and resources to both spread the word and launch a high quality gaming platform." },
			        { Q: "Who can play on <span class=\"text-gz\"><b>greenzorro</b></span>?", A: "Anyone located in an eligible jurisdiction and within legal gambling age can enjoy the benefits of <span class=\"text-gz\"><b>greenzorro</b></span>." },
			        //{ Q: "When will <span class=\"text-gz\"><b>greenzorro</b></span> start accepting bets?", A: "<span class=\"text-gz\"><b>greenzorro</b></span> expects to start sending registration invitations in January 2016 and launch in March 2016." }
                ]
            },
            //{
            //    name: 'Games',
            //    items: [
			//        { Q: "What Casino games are available", A: "<span class=\"text-gz\"><b>greenzorro</b></span> will offer the most popular casino games: Blackjack, Roulette, Slot machines, Baccarat, all of which will be renewed on a frequent basis. Across all games, <span class=\"text-gz\"><b>greenzorro</b></span> will offer the best possible odds for the players, maximizing payout." },
			//        { Q: "What about Sports Betting?", A: "<span class=\"text-gz\"><b>greenzorro</b></span> will offer a complete sports book featuring the most popular sports worldwide." }
            //    ]
            //},
            {
                name: 'Investments',
                items: [
			        { Q: "How can players recoup their losses through investments?", A: "An investment is defined as <i>'putting money into an asset with the expectation of capital appreciation, dividends, and/or interest earnings'</i>. Invested funds, if invested correctly should grow every year, and allow players to make gains over the original amount." },
			        { Q: "How much money can I get back?", A: "Depending on investment hold period, you can recoup a minimum of 50% of your losses and up to 110%. For example, if you bet &euro;1,000 and lose, you can recoup from &euro;500 to &euro;1,100, depending on your investment hold period and subject to investment performance." },
			        { Q: "Do I need to be a sophisticated investor to invest to my losses?", A: "Not at all. <span class=\"text-gz\"><b>greenzorro</b></span> will feature a sophisticated and user friendly investment dashboard, with pre-selected options, to make decisions easier and smoother for the users." },
			        { Q: "Is there any risk?", A: "Most investments involve some kind of risk, but these are related to inflation, market conditions and other macro and micro economic factors. <span class=\"text-gz\"><b>greenzorro</b></span> will select investment options that minimize risk, ensuring that your capital will grow constantly." },
			        { Q: "Who makes the investment decisions?", A: "Users can choose where to allocate their funds depending on their investment horizon: short (1-2 years), medium (3-5 years) or long (6+ years). The actual portfolios will be pre-selected by the <span class=\"text-gz\"><b>greenzorro</b></span> investment team in cooperation with the worlds most experienced investment professionals. All portfolios are well diversified to ensure high return and low risk for the selected investment horizon." },
			        { Q: "What are some typical products that <span class=\"text-gz\"><b>greenzorro</b></span> will invest in?", A: "All investment portfolios are primarily comprised from stocks and bonds. For stocks, <span class=\"text-gz\"><b>greenzorro</b></span> ensures high diversification by selecting index funds across different markets. On the bonds side, <span class=\"text-gz\"><b>greenzorro</b></span> will select a wide mix of corporate and governmental bonds to maximize returns while keeping risks low. Different investment horizons have different mix of stocks and bonds with longer term investments having a higher percentage of stocks." },
			        { Q: "What is the typical investment period", A: "The investment period is decided by the user. Users can choose from three options depending on their investment horizon: short (1-2 years), medium (3-5 years) or long (6+ years). Longer investment periods typically yield higher returns." },
			        { Q: "Can I get money before the end of the investment period?", A: "Yes. You can get your money back at any point in time and before the investment period. However, liquidating the investment before the end of the investment period you selected will incur some transaction fees." },
			        { Q: "How do I cash out?", A: "You can cash out through your investment dashboard in the <span class=\"text-gz\"><b>greenzorro</b></span> website with a simple click of a button. greenzorro will credit your account within 2 days." },
			        //{ Q: "Are you regulated by any financial authority such as the FCA?", A: "<span class=\"text-gz\"><b>greenzorro</b></span> is in the process of registering its investment activities with the FCA. Additionally, all investment managers/ companies that <span class=\"text-gz\"><b>greenzorro</b></span> allocates funds in are regulated by the FCA or the relevant authority in each jurisdiction." },
			        { Q: "How can I be reassured that I will get my money from investments?", A: "All registered users at the <span class=\"text-gz\"><b>greenzorro</b></span> platform will automatically be registered as unit holders in the fund that will hold their portfolio. This means that in the unlikely event of a bankruptcy, your investments are protected, and always belong to you." },
			        { Q: "Where can I see the status of my investments?", A: "Once you become a registered user, you can have access to the <span class=\"text-gz\"><b>greenzorro</b></span> platform, where you can view all investment activities through the investment dashboard. There, you can find a full view of the funds you have deposited, how much money has been transferred to your investment account, and the current status of your investments." }
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