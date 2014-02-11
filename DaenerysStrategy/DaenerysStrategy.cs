using System;
using System.Collections.Generic;
using TradingMotion.SDK.Algorithms;
using TradingMotion.SDK.Algorithms.InputParameters;
using TradingMotion.SDK.Markets.Charts;
using TradingMotion.SDK.Markets.Indicators.Momentum;
using TradingMotion.SDK.Markets.Indicators.StatisticFunctions;

using TradingMotion.SDK.Markets.Orders;

/// <summary>
/// Daenerys trading rules:
///   * Entry: Price breaks above RSI buy signal level (long entry) or below RSI sell signal level (short entry)
///   * Exit: Reversal RSI sell/buy signal or fixed Stop Loss order
///   * Filters: None
/// </summary>
namespace DaenerysStrategy
{
    public class DaenerysStrategy : Strategy
    {
        RSIIndicator rsiIndicator;
        StopOrder catastrophicStop;

        public DaenerysStrategy(Chart mainChart, List<Chart> secondaryCharts)
            : base(mainChart, secondaryCharts)
        {

        }

        /// <summary>
        /// Strategy Name
        /// </summary>
        /// <returns>The complete name of the strategy</returns>
        public override string Name
        {
            get
            {
                return "Daenerys Strategy";
            }
        }

        /// <summary>
        /// Security filter that ensures the Position will be closed at the end of the trading session.
        /// </summary>
        public override bool ForceCloseIntradayPosition
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Security filter that sets a maximum open position size of 1 contract (either side)
        /// </summary>
        public override uint MaxOpenPosition
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// This strategy uses the Advanced Order Management mode
        /// </summary>
        public override bool UsesAdvancedOrderManagement
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Strategy Parameter definition
        /// </summary>
        public override InputParameterList SetInputParameters()
        {
            InputParameterList parameters = new InputParameterList();

            // The previous N bars period RSI indicator will use
            parameters.Add(new InputParameter("RSI Period", 80));

            // Break level of RSI indicator we consider a buy signal
            parameters.Add(new InputParameter("RSI Buy signal trigger level", 52));
            // Break level of RSI indicator we consider a sell signal
            parameters.Add(new InputParameter("RSI Sell signal trigger level", 48));

            // The distance between the entry and the fixed stop loss order
            parameters.Add(new InputParameter("Catastrophic Stop Loss ticks distance", 58));

            return parameters;
        }

        /// <summary>
        /// Initialization method
        /// </summary>
        public override void OnInitialize()
        {
            log.Debug("DaenerysStrategy onInitialize()");

            // Adding an RSI indicator to strategy 
            // (see http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi)
            rsiIndicator = new RSIIndicator(Bars.Close, (int)this.GetInputParameter("RSI Period"), (int)this.GetInputParameter("RSI Sell signal trigger level"), (int)this.GetInputParameter("RSI Buy signal trigger level"));
            this.AddIndicator("RSI indicator", rsiIndicator);

        }

        /// <summary>
        /// Strategy enter/exit/filtering rules
        /// </summary>
        public override void OnNewBar()
        {
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
        }
    }
}
