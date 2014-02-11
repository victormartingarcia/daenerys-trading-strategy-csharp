Daenerys Trading Strategy
============================================

Table of Contents
----

* [Overview](#overview)
* [Daenerys Trading Rules](#daenerys-trading-rules)
* [Download](#download)
* [Quick Start](#quick-start)
* [User Manual](#user-manual)
* [About iSystems](#about-isystems)
* [Disclaimer](#disclaimer)

Overview
----

Daenerys is a trading algorithm written in C# using the [TradingMotion SDK] development tools (there is a [VB.net] port too).

![Elite OHLC example chart](markdown_files/Elite_OHLC.png)
<sub>__Image footnote:__ Example of Daenerys OHLC financial chart showing some automatic trades</sub>

The strategy code is all contained in [Daenerys.cs], including a default parameter combination.

This default parameter combination has been optimized to run over 120' bars of _Mini-Russell Future Index_.

Trading a maximum of 1 contract of Mini-Russel Future, it performed a hypothetical +7.65% annual ROI since 2001, calculated over the suggested $12000 initial capital.

Anyway, go open Visual Studio, clone the project and start with the trading algo development! Sure you can do better and improve all these figures :)

Daenerys Trading Rules
----

Daenerys' trading plan is quite simple. When flat, it __buys 1 contract__ when the price breaks above a specified RSI's level, or __sells 1 contract__ when the price breaks belowe another RSI's level.

While the strategy has a position in the market, it __places a Stop Loss__ order (accept we were wrong on our prediction and cut the losses). Besides, if a reversal signal is triggered (the contrary RSI signal entry level is broken) it closes the current position.

Besides, this is a pure __intraday strategy__. That means it won't leave any open position at the end of the session, so in case we still got a position it will be closed automatically.

### To sum up ###
```
DaenerysStrategy rules:

  * Entry: Price breaks above RSI buy level (long entry) or price breaks below RSI sell level (short entry)
  * Exit: Set a fixed Stop Loss and closes the position on a reversal signal
  * Filters (sets the entry only under certain conditions): None
```

### Show me the code ###

Here is a simplified C# source code of Daenerys' _OnNewBar()_ function. The complete code is all contained in [DaenerysStrategy.cs] along with comments and definition of parameters.

```csharp
decimal stopMargin = (int)this.GetInputParameter("Catastrophic Stop Loss ticks distance") * this.GetMainChart().Symbol.TickSize;

int buySignal = (int)this.GetInputParameter("RSI Buy signal trigger level");
int sellSignal = (int)this.GetInputParameter("RSI Sell signal trigger level");

if (rsiIndicator.GetRSI()[1] <= buySignal && rsiIndicator.GetRSI()[0] > buySignal && this.GetOpenPosition() != 1)
{
    if (this.GetOpenPosition() == 0)
    {
        //BUY SIGNAL: Entering long and placing a catastrophic stop loss
        MarketOrder buyOrder = new MarketOrder(OrderSide.Buy, 1, "Enter long position");
        catastrophicStop = new StopOrder(OrderSide.Sell, 1, this.Bars.Close[0] - stopMargin, "Catastrophic stop long exit");

        this.InsertOrder(buyOrder);
        this.InsertOrder(catastrophicStop);
    }
    else if (this.GetOpenPosition() == -1)
    {
        //BUY SIGNAL: Closing short position and cancelling the catastrophic stop loss order
        MarketOrder exitShortOrder = new MarketOrder(OrderSide.Buy, 1, "Exit short position (reversal exit signal)");

        this.InsertOrder(exitShortOrder);
        this.CancelOrder(catastrophicStop);
    }
}
else if (rsiIndicator.GetRSI()[1] >= sellSignal && rsiIndicator.GetRSI()[0] < sellSignal && this.GetOpenPosition() != -1)
{
    if (this.GetOpenPosition() == 0)
    {
        //SELL SIGNAL: Entering short and placing a catastrophic stop loss
        MarketOrder sellOrder = new MarketOrder(OrderSide.Sell, 1, "Enter short position");
        catastrophicStop = new StopOrder(OrderSide.Buy, 1, this.Bars.Close[0] + stopMargin, "Catastrophic stop short exit");

        this.InsertOrder(sellOrder);
        this.InsertOrder(catastrophicStop);
    }
    else if (this.GetOpenPosition() == 1)
    {
        //SELL SIGNAL: Closing long position and cancelling the catastrophic stop loss order
        MarketOrder exitLongOrder = new MarketOrder(OrderSide.Sell, 1, "Exit long position (reversal exit signal)");

        this.InsertOrder(exitLongOrder);
        this.CancelOrder(catastrophicStop);
    }
}
```

Download
----

First of all, make sure you have Visual Studio 2010 version (or higher). [TradingMotion SDK] is fully compatible with [Visual Studio Express] free versions.

Download TradingMotion [Visual Studio extension], and the windows desktop application [TradingMotionSDK Toolkit installer].


Quick Start
----

* Create a free account to access TradingMotionAPI (required). It can be created from TradingMotionSDK Toolkit (the desktop application)
* Clone the repository:
```sh
git clone https://github.com/victormartingarcia/daenerys-trading-strategy-csharp
```
* Open Visual Studio and load solution _DaenerysStrategy/DaenerysStrategy.sln_
* Edit _app.config_ file adding your TradingMotionAPI credentials on _appSettings_ section

And you're all set!

Running the project (F5) will perform a _development backtest simulation_ over last 6 months DAX 60' bars data.

Once it has finished, it will ask if you want to see the P&L report in TradingMotionSDK Toolkit. Pressing 'y' will load the same backtest with the desktop app, where it will show performance statistics, charts, and so on.

![Elite Scatter Plot](markdown_files/Elite_Scatter_Plot.png)
<sub>__Image footnote:__ Scatter Plot 6 month backtest screenshot. It shows the net profit for each session in the simulation. In this case the best one was 13th november 2013, where it got a hypothetical net profit of $934</sub>

User Manual
----

__[More documentation in the Getting Started Guide]__

About iSystems
----

[iSystems] by [TradingMotion] is a marketplace for automated trading systems.

_iSystems_ has partnered with [11 international brokers](http://www.tradingmotion.com/Brokers) (and counting) that offer these trading systems to their clients (both corporate and retail) who pay for a license fee that the developer charges.

The trading systems run with live market data under a controlled environment in iSystems' datacenters.

This way the developers just need to worry about how to make their trading systems better and iSystems platform does the rest.

Visit [Developers] section on TradingMotion's website for more info on how to develop and offer your systems.

Disclaimer
----

I am R&D engineer at [TradingMotion LLC], and head of [TradingMotion SDK] platform. Beware, the info here can be a little biased ;)

  [VB.net port]: https://github.com/victormartingarcia/daenerys-trading-strategy-vbnet
  [TradingMotion SDK]: http://sdk.tradingmotion.com
  [DaenerysStrategy.cs]: DaenerysStrategy/DaenerysStrategy.cs
  [iSystems platform]: https://www.isystems.com
  [iSystems.com]: https://www.isystems.com
  [iSystems]: https://www.isystems.com
  [Intr Elite 10' MR 2.0 -0.1]: https://automated.isystems.com/Systems/PerformanceSheet/10297
  [TradingMotion LLC]: http://www.tradingmotion.com
  [TradingMotion]: http://www.tradingmotion.com
  [Developers]: http://www.tradingmotion.com/Strategies/Developers
  [Visual Studio Express]: http://www.visualstudio.com/en-us/downloads#d-2010-express
  [TradingMotion SDK website]: http://sdk.tradingmotion.com
  [TradingMotionSDK Toolkit installer]: http://sdk.tradingmotion.com/files/TradingMotionSDKInstaller.msi
  [Visual Studio extension]: http://sdk.tradingmotion.com/files/TradingMotionSDK_VisualStudio.vsix
  [More documentation in the Getting Started Guide]: http://sdk.tradingmotion.com/GettingStarted
