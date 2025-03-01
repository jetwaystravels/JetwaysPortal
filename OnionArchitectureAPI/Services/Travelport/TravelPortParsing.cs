﻿using DomainLayer.Model;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Xml;
using static DomainLayer.Model.GDSResModel;

namespace OnionArchitectureAPI.Services.Travelport
{
    public class TravelPortParsing
    {
        ArrayList listOfFareDetails = null;
        List<GDSResModel.Segment> listOfSegment = new List<GDSResModel.Segment>();
        //ArrayList listofTPSegment = null;
        ArrayList listOfBound = new ArrayList();
        int contractId = 1;
        string TPtransactionId = string.Empty;
        string outBoundGroup = string.Empty;
        string inBoundGroup = string.Empty;
        int journeyIndex = 0;
        bool IsFlex = true;
        string _journeyTime = string.Empty;
        //List<Bond> listOfBound = new List<Bond>();
        GDSResModel.Segment seg = null;
        bool airSegstatus = false;
        XmlDocument flightDetailsList = new XmlDocument();
        XmlDocument FareInfoList = new XmlDocument();
        XmlDocument BrandList = null;
        string fareRoues = string.Empty;
        GDSResModel.Fare fare = null;
        Dictionary<string, string> baggageDetais = new Dictionary<string, string>();
        Dictionary<string, string> fareRuleInfo = new Dictionary<string, string>();
        //string contractType_ = "OneWay";

        GDSResModel.Leg leg = null;
        GDSResModel.PaxFare paxFare = new PaxFare();
        FareDetail fareDetails = new FareDetail();
        bool IsDomestic = true;
        ArrayList listofTPSegment = null;
        GDSResModel.TPAirSegment AirSegment = null;
        string TraceId = string.Empty;
        string AirPricingSolutinForPNR = string.Empty;
        StringBuilder NewPricingSolutionValue = null;
        string OldPricingSolution = string.Empty;
        // bool airPricingsolution = false;
        string group = string.Empty;
        string adultPricingInfo = string.Empty;
        string childPricingInfo = string.Empty;
        string infantInfo = string.Empty;
        string pricingInfo = string.Empty;
        GDSResModel.Bond finalBond = null;
        public List<GDSResModel.Segment> ParseAirFareRsp(string AirFareResponse, string contractType_, SimpleAvailabilityRequestModel availibiltyRQGDS)
        {

            try
            {
                listOfSegment = new List<GDSResModel.Segment>();
                listOfBound = new ArrayList();
                listofTPSegment = new ArrayList();
                GDSResModel.Bond bond = new GDSResModel.Bond();
                bond = new GDSResModel.Bond();
                bond.Legs = new List<GDSResModel.Leg>();
                XmlDocument doc = new XmlDocument();
                XmlDocument airSegmentList = new XmlDocument();
                if (!string.IsNullOrEmpty(AirFareResponse))
                {
                    doc.LoadXml(AirFareResponse);
                }
                //cancelTime = new FareCancellationTime();
                foreach (XmlNode rootNode in doc)
                {
                    if (rootNode.Name.Equals("SOAP:Envelope", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XmlNode root in rootNode.ChildNodes)
                        {
                            if (root.Name.Equals("SOAP:Body", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (XmlNode airPriceRes in root)
                                {
                                    if (airPriceRes.Name.Equals("air:AirPriceRsp", StringComparison.OrdinalIgnoreCase))
                                    {
                                        TPtransactionId = airPriceRes.Attributes["TransactionId"].InnerText;
                                        TraceId = airPriceRes.Attributes["TraceId"].Value;
                                        foreach (XmlNode airPriceResChild in airPriceRes)
                                        {
                                            if (airPriceResChild.Name.Equals("air:AirItinerary", StringComparison.OrdinalIgnoreCase))
                                            {
                                                airSegmentList.LoadXml((airPriceResChild).OuterXml);
                                            }
                                            #region AirPricingSolution
                                            if (airPriceResChild.Name.Equals("air:AirPriceResult", StringComparison.OrdinalIgnoreCase))
                                            {
                                                int _count = 0;
                                                foreach (XmlNode airPricingSoulution in airPriceResChild)
                                                {
                                                    bool apsPNRStatus = true;
                                                    if (airPricingSoulution.Name.Equals("air:AirPricingSolution", StringComparison.OrdinalIgnoreCase) && _count == 0)
                                                    {
                                                        _count = 1;
                                                        OldPricingSolution = airPricingSoulution.OuterXml;
                                                        fare = new GDSResModel.Fare();
                                                        decimal TMarkup = 0;
                                                        if (airPricingSoulution.Attributes["BasePrice"].Value.Contains("INR"))
                                                        {
                                                            fare.BasicFare = Convert.ToDecimal(airPricingSoulution.Attributes["BasePrice"].Value.Remove(0, 3));
                                                        }
                                                        else
                                                        {
                                                            fare.BasicFare = Convert.ToDecimal(airPricingSoulution.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                        }
                                                        if (airPricingSoulution.Attributes["Taxes"].Value.Contains("INR"))
                                                        {
                                                            //TMarkup = GetMarkup(Convert.ToDecimal(airPricingSoulution.Attributes["Taxes"].Value.Remove(0, 3)), Convert.ToDecimal(airPricingSoulution.Attributes["Taxes"].Value.Remove(0, 3)), Convert.ToDecimal(airPricingSoulution.Attributes["Taxes"].Value.Remove(0, 3)));
                                                            fare.TotalTaxWithOutMarkUp = Convert.ToDecimal(airPricingSoulution.Attributes["Taxes"].Value.Remove(0, 3)) + TMarkup;
                                                        }
                                                        else
                                                        {
                                                            //TMarkup = GetMarkup(Convert.ToDecimal(airPricingSoulution.Attributes["Taxes"].Value.Remove(0, 3)), Convert.ToDecimal(airPricingSoulution.Attributes["Taxes"].Value.Remove(0, 3)), Convert.ToDecimal(airPricingSoulution.Attributes["Taxes"].Value.Remove(0, 3)));
                                                            fare.TotalTaxWithOutMarkUp = Convert.ToDecimal(airPricingSoulution.Attributes["Taxes"].Value.Remove(0, 3)) + TMarkup;
                                                        }

                                                        if (airPricingSoulution.Attributes["TotalPrice"].Value.Contains("INR"))
                                                        {
                                                            fare.TotalFareWithOutMarkUp = Convert.ToDecimal(airPricingSoulution.Attributes["TotalPrice"].Value.Remove(0, 3)) + TMarkup;
                                                        }
                                                        else
                                                        {
                                                            fare.TotalFareWithOutMarkUp = Convert.ToDecimal(airPricingSoulution.Attributes["ApproximateTotalPrice"].Value.Remove(0, 3)) + TMarkup;
                                                        }
                                                        fare.PaxFares = new List<PaxFare>();
                                                        foreach (XmlNode lowfarepric in airPricingSoulution)
                                                        {
                                                            if (apsPNRStatus)
                                                            {
                                                                AirPricingSolutinForPNR = airPricingSoulution.OuterXml;
                                                                apsPNRStatus = false;
                                                            }
                                                            switch (lowfarepric.Name)
                                                            {
                                                                #region airJourney
                                                                case "air:AirSegmentRef":
                                                                    if (lowfarepric.Name.Equals("air:AirSegmentRef", StringComparison.OrdinalIgnoreCase))
                                                                    {
                                                                        leg = new GDSResModel.Leg();
                                                                        if (airSegmentList != null && !string.IsNullOrEmpty(airSegmentList.InnerXml))
                                                                        {
                                                                            foreach (XmlNode airSegmentlist in airSegmentList)
                                                                            {
                                                                                if (airSegmentlist.Name.Equals("air:AirItinerary", StringComparison.OrdinalIgnoreCase))
                                                                                {
                                                                                    foreach (XmlNode airSegment in airSegmentlist)
                                                                                    {
                                                                                        if (airSegment.Name.Equals("air:AirSegment", StringComparison.OrdinalIgnoreCase))
                                                                                        {

                                                                                            if (airSegment.Attributes["Key"].Value.Equals(lowfarepric.Attributes["Key"].InnerText, StringComparison.OrdinalIgnoreCase))
                                                                                            {
                                                                                                AirSegment = new TPAirSegment();
                                                                                                AirSegment.AirSegment = airSegment.Attributes["Key"].Value;
                                                                                                AirSegment.AirSegmentDetail = airSegment.OuterXml.Trim();
                                                                                                listofTPSegment.Add(AirSegment);
                                                                                                //if (contractType_.Equals("RoundTrip"))
                                                                                                //{
                                                                                                //    leg.BoundType = "OutBound";
                                                                                                //}
                                                                                                //else
                                                                                                //{
                                                                                                //    leg.BoundType = contractType_;
                                                                                                //}
                                                                                                //added
                                                                                                if (airSegment.Attributes["Group"].Value.Equals("0", StringComparison.OrdinalIgnoreCase))
                                                                                                {
                                                                                                    leg.BoundType = "OutBound";
                                                                                                    leg.Group = "0";
                                                                                                    //bond.BoundType = "OutBound";
                                                                                                }
                                                                                                else if (airSegment.Attributes["Group"].Value.Equals("1", StringComparison.OrdinalIgnoreCase))
                                                                                                {
                                                                                                    leg.BoundType = "InBound";
                                                                                                    leg.Group = "1";
                                                                                                    //bond.BoundType = "InBound";
                                                                                                }
                                                                                                if (airSegment.Attributes["NumberOfStops"] != null)
                                                                                                {
                                                                                                    leg.NumberOfStops = airSegment.Attributes["NumberOfStops"].Value;
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    leg.NumberOfStops = "0";
                                                                                                }
                                                                                                //
                                                                                                leg.FlightNumber = airSegment.Attributes["FlightNumber"].Value;
                                                                                                leg.AirlineName = airSegment.Attributes["Carrier"].Value;
                                                                                                leg.CarrierCode = airSegment.Attributes["Carrier"].Value;
                                                                                                leg.Origin = airSegment.Attributes["Origin"].Value;
                                                                                                leg.Destination = airSegment.Attributes["Destination"].Value;
                                                                                                //leg.DepartureDate = Utility.Utility.GDateFormate(airSegment.Attributes["DepartureTime"].Value.Split('T')[0], Utility.Engine.TravelPort);
                                                                                                leg.DepartureTime = airSegment.Attributes["DepartureTime"].Value;// airSegment.Attributes["DepartureTime"].Value.Split('T')[1];
                                                                                                //leg.ArrivalDate = Utility.Utility.GDateFormate(airSegment.Attributes["ArrivalTime"].Value.Split('T')[0], Utility.Engine.TravelPort);
                                                                                                leg.ArrivalTime = airSegment.Attributes["ArrivalTime"].Value;// airSegment.Attributes["ArrivalTime"].Value.Split('T')[1];
                                                                                                leg.FareClassOfService = airSegment.Attributes["ClassOfService"].Value;
                                                                                                if(airSegment.Attributes["Equipment"]!=null)
                                                                                                  leg._Equipment= airSegment.Attributes["Equipment"].Value;
                                                                                                if (airSegment.Attributes["FlightTime"] != null)
                                                                                                {
                                                                                                    leg.Duration = airSegment.Attributes["FlightTime"].Value;
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    leg.Duration = "120";
                                                                                                }
                                                                                                leg.AircraftCode = airSegment.Attributes["Key"].InnerText;
                                                                                                leg.Group = airSegment.Attributes["Group"].Value;
                                                                                                foreach (XmlNode airSegmentChild in airSegment)
                                                                                                {
                                                                                                    switch (airSegmentChild.Name)
                                                                                                    {
                                                                                                        case "air:AirAvailInfo":
                                                                                                            leg.ProviderCode = airSegmentChild.Attributes["ProviderCode"].Value;
                                                                                                            break;
                                                                                                        case "air:FlightDetailsRef":
                                                                                                            leg.FlightDetailRefKey = airSegmentChild.Attributes["Key"].Value;
                                                                                                            break;
                                                                                                    }
                                                                                                }
                                                                                                switch (leg.AirlineName)
                                                                                                {
                                                                                                    case "AI":
                                                                                                        leg.FlightName = "AirIndia";
                                                                                                        break;
                                                                                                    case "9W":
                                                                                                        leg.FlightName = "JetAirWays";
                                                                                                        break;
                                                                                                    case "UK":
                                                                                                        leg.FlightName = "Vistara";
                                                                                                        break;
                                                                                                    case "OD":
                                                                                                        leg.FlightName = "MALINDO AIRWAYS";
                                                                                                        break;
                                                                                                    case "TG":
                                                                                                        leg.FlightName = "Thai Airways International";
                                                                                                        break;
                                                                                                    case "UL":
                                                                                                        leg.FlightName = "SriLankan Airlines";
                                                                                                        break;
                                                                                                    default:
                                                                                                        try
                                                                                                        {
                                                                                                            //leg.FlightName = Utility.FileChangeMonitor.AirlineNames[leg.AirlineName.Trim()];
                                                                                                        }
                                                                                                        catch (SystemException sex_) { leg.FlightName = leg.AirlineName.Trim(); }
                                                                                                        break;
                                                                                                }
                                                                                                bond.Legs.Add(leg);
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    break;
                                                                #endregion
                                                                #region airPricingInfo
                                                                case "air:AirPricingInfo":

                                                                    paxFare = new PaxFare();
                                                                    paxFare.Fare = new List<FareDetail>();
                                                                    if (lowfarepric.Attributes["Refundable"] != null)
                                                                    {
                                                                        paxFare.Refundable = bool.Parse(lowfarepric.Attributes["Refundable"].Value);
                                                                    }
                                                                    foreach (XmlNode bookingInfo in lowfarepric)
                                                                        if (bookingInfo.Name.Equals("air:BookingInfo", StringComparison.OrdinalIgnoreCase))
                                                                        {

                                                                            foreach (GDSResModel.Leg leg_ in bond.Legs)
                                                                            {
                                                                                if (leg_.AircraftCode.Equals(bookingInfo.Attributes["SegmentRef"].Value, StringComparison.OrdinalIgnoreCase))
                                                                                {
                                                                                    leg_.FareClassOfService = bookingInfo.Attributes["BookingCode"].Value;
                                                                                    leg_.Cabin = bookingInfo.Attributes["CabinClass"].Value;
                                                                                    leg_.FareRulesKey = bookingInfo.Attributes["FareInfoRef"].Value;

                                                                                }
                                                                            }

                                                                        }
                                                                    foreach (XmlNode airPricingInfo in lowfarepric)
                                                                    {

                                                                        #region fareBreckkup
                                                                        if (airPricingInfo.Name.Equals("air:TaxInfo", StringComparison.OrdinalIgnoreCase))
                                                                        {
                                                                            if (airPricingInfo.Attributes["Category"] != null && airPricingInfo.Attributes["Amount"] != null)
                                                                            {
                                                                                fareDetails = new FareDetail();
                                                                                fareDetails.Amount = decimal.Parse(airPricingInfo.Attributes["Amount"].Value.Remove(0, 3));
                                                                                switch (airPricingInfo.Attributes["Category"].Value)
                                                                                {
                                                                                    case "IN":
                                                                                        fareDetails.ChargeCode = "UDF";
                                                                                        fareDetails.ChargeDetail = "USER DEVELOPMENT FEE";
                                                                                        break;
                                                                                    case "JN":
                                                                                        fareDetails.ChargeCode = "ST";
                                                                                        fareDetails.ChargeDetail = "SERVICE TAX";
                                                                                        break;
                                                                                    case "WO":
                                                                                        fareDetails.ChargeCode = "PSF";
                                                                                        fareDetails.ChargeDetail = "PASSENGER SERVICE FEE";
                                                                                        break;
                                                                                    case "YM":
                                                                                        fareDetails.ChargeCode = "ADF";
                                                                                        fareDetails.ChargeDetail = "AIRPORT DEVELOPMENT FEE";
                                                                                        break;
                                                                                    case "YQ":
                                                                                        fareDetails.ChargeCode = "YQ";
                                                                                        fareDetails.ChargeDetail = "Fuel Expenses";
                                                                                        break;
                                                                                    case "YR":
                                                                                        fareDetails.ChargeCode = "YR";
                                                                                        fareDetails.ChargeDetail = "Fuel Expenses";
                                                                                        break;
                                                                                    default:
                                                                                        fareDetails.ChargeCode = airPricingInfo.Attributes["Category"].Value;
                                                                                        fareDetails.ChargeDetail = airPricingInfo.Attributes["Category"].Value;
                                                                                        break;
                                                                                }
                                                                                paxFare.Fare.Add(fareDetails);
                                                                            }
                                                                        }

                                                                        #endregion


                                                                        if (airPricingInfo.Name.Equals("air:PassengerType", StringComparison.OrdinalIgnoreCase))
                                                                        {
                                                                            switch (airPricingInfo.Attributes["Code"].Value)
                                                                            {
                                                                                case "ADT":
                                                                                    if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                                    paxFare.PaxType = PAXTYPE.ADT;
                                                                                    //if (AdultMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                                    //{
                                                                                    //paxFare.TotalTax += AdultMarkUp;
                                                                                    //paxFare.Fare[0].Amount += AdultMarkUp;
                                                                                    //}
                                                                                    //if (MarkUpType == MarkUP.Percentage && AdultMarkUp != 0)
                                                                                    //{
                                                                                    //paxFare.TotalTax += (paxFare.TotalTax * (AdultMarkUp / 100));
                                                                                    //paxFare.Fare[0].Amount += (paxFare.TotalTax * (AdultMarkUp / 100));
                                                                                    //}
                                                                                    break;
                                                                                case "CNN":
                                                                                    if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                                    paxFare.PaxType = PAXTYPE.CHD;
                                                                                    //if (ChildMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                                    //{
                                                                                    //    paxFare.TotalTax += ChildMarkUp;
                                                                                    //    paxFare.Fare[0].Amount += ChildMarkUp;
                                                                                    //}
                                                                                    //if (MarkUpType == MarkUP.Percentage && ChildMarkUp != 0)
                                                                                    //{
                                                                                    //    paxFare.TotalTax += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                                    //    paxFare.Fare[0].Amount += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                                    //}
                                                                                    break;
                                                                                case "C11":
                                                                                    if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                                    paxFare.PaxType = PAXTYPE.CHD;
                                                                                    //if (ChildMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                                    //{
                                                                                    //    paxFare.TotalTax += ChildMarkUp;
                                                                                    //    paxFare.Fare[0].Amount += ChildMarkUp;
                                                                                    //}
                                                                                    //if (MarkUpType == MarkUP.Percentage && ChildMarkUp != 0)
                                                                                    //{
                                                                                    //    paxFare.TotalTax += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                                    //    paxFare.Fare[0].Amount += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                                    //}
                                                                                    break;
                                                                                case "CHD":
                                                                                    if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                                    paxFare.PaxType = PAXTYPE.CHD;
                                                                                    //if (ChildMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                                    //{
                                                                                    //    paxFare.TotalTax += ChildMarkUp;
                                                                                    //    paxFare.Fare[0].Amount += ChildMarkUp;
                                                                                    //}
                                                                                    //if (MarkUpType == MarkUP.Percentage && ChildMarkUp != 0)
                                                                                    //{
                                                                                    //    paxFare.TotalTax += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                                    //    paxFare.Fare[0].Amount += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                                    //}
                                                                                    break;
                                                                                case "INF":
                                                                                    if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                                    }
                                                                                    paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                                    paxFare.PaxType = PAXTYPE.INF;
                                                                                    //if (InfantMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                                    //{
                                                                                    //    paxFare.TotalTax += InfantMarkUp;
                                                                                    //    paxFare.Fare[0].Amount += InfantMarkUp;
                                                                                    //}
                                                                                    //if (MarkUpType == MarkUP.Percentage && InfantMarkUp != 0)
                                                                                    //{
                                                                                    //    paxFare.TotalTax += (paxFare.TotalTax * (InfantMarkUp / 100));
                                                                                    //    paxFare.Fare[0].Amount += (paxFare.TotalTax * (InfantMarkUp / 100));
                                                                                    //}
                                                                                    break;
                                                                            }
                                                                        }
                                                                        if (airPricingInfo.Name.Equals("air:ChangePenalty", StringComparison.OrdinalIgnoreCase))
                                                                        {
                                                                            foreach (XmlNode changePenalty in airPricingInfo)
                                                                            {
                                                                                if (changePenalty.Name.Equals("air:Amount", StringComparison.OrdinalIgnoreCase))
                                                                                {
                                                                                    paxFare.ChangePenalty = (Convert.ToDecimal(changePenalty.InnerText.Remove(0, 3))) * bond.Legs.Count;
                                                                                }
                                                                            }
                                                                        }
                                                                        if (airPricingInfo.Name.Equals("air:CancelPenalty", StringComparison.OrdinalIgnoreCase))
                                                                        {
                                                                            foreach (XmlNode cancelPenalty in airPricingInfo)
                                                                            {
                                                                                if (cancelPenalty.Name.Equals("air:Amount", StringComparison.OrdinalIgnoreCase))
                                                                                {
                                                                                    paxFare.CancelPenalty = Convert.ToDecimal(cancelPenalty.InnerText.Remove(0, 3)) * bond.Legs.Count;// * ((Bond)listOfBound[0]).Legs.Count
                                                                                    if (paxFare.CancelPenalty == 0)
                                                                                    {
                                                                                        paxFare.Refundable = false;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        //
                                                                        try
                                                                        {
                                                                            if (airPricingInfo.Name.Equals("air:BaggageAllowances", StringComparison.OrdinalIgnoreCase))
                                                                            {
                                                                                foreach (XmlNode baggageInfo in airPricingInfo)
                                                                                {
                                                                                    if (baggageInfo.Name.Equals("air:BaggageAllowanceInfo", StringComparison.OrdinalIgnoreCase))
                                                                                    {
                                                                                        foreach (XmlNode BagDetails in baggageInfo)
                                                                                        {
                                                                                            if (BagDetails.Name.Equals("air:TextInfo", StringComparison.OrdinalIgnoreCase))
                                                                                            {
                                                                                                foreach (XmlNode BagText in BagDetails)
                                                                                                {
                                                                                                    if (BagText.Name.Equals("air:Text", StringComparison.OrdinalIgnoreCase))
                                                                                                    {
                                                                                                        if (BagText.InnerText.Contains("K"))
                                                                                                        {
                                                                                                            paxFare.BaggageWeight = BagText.InnerText.Replace("K", "");
                                                                                                            paxFare.BaggageUnit = "KG";
                                                                                                        }
                                                                                                        if (BagText.InnerText.Contains("P"))
                                                                                                        {
                                                                                                            paxFare.BaggageWeight = BagText.InnerText.Replace("P", "");
                                                                                                            paxFare.BaggageUnit = "PC";
                                                                                                        }
                                                                                                        break;
                                                                                                    }
                                                                                                }

                                                                                            }

                                                                                        }

                                                                                    }

                                                                                }
                                                                            }
                                                                        }
                                                                        catch
                                                                        {

                                                                        }

                                                                        //
                                                                    }
                                                                    paxFare.TotalFare = paxFare.BasicFare + paxFare.TotalTax;
                                                                    fare.PaxFares.Add(paxFare);
                                                                    break;
                                                                    #endregion
                                                            }
                                                        }
                                                    }


                                                    if (bond.Legs.Count > 0)
                                                    {
                                                        seg = new GDSResModel.Segment();
                                                        seg.Bonds = new List<Bond>();
                                                        seg.Fare = new GDSResModel.Fare();
                                                        seg.Fare = fare;
                                                        seg.SegIndex = contractId.ToString();
                                                        //seg.FareIndicator = FareIndicator;
                                                        bool groupStatus = false;
                                                        if (contractType_.Equals("RoundTrip"))
                                                        {
                                                            seg.BondType = "OutBound";
                                                            seg.IsRoundTrip = true;
                                                            for (int bondcount = 0; bondcount <= 1; bondcount++)
                                                            {
                                                                finalBond = new Bond();
                                                                finalBond.Legs = new List<GDSResModel.Leg>();
                                                                groupStatus = false;
                                                                foreach (GDSResModel.Leg leg_ in bond.Legs)
                                                                {
                                                                    if (!groupStatus && string.IsNullOrEmpty(group))
                                                                    {
                                                                        group = leg_.Group;
                                                                        groupStatus = true;
                                                                        finalBond.Legs.Add(leg_);
                                                                        //Added
                                                                        if (leg_.Group == "0")
                                                                        {
                                                                            finalBond.BoundType = "OutBound";
                                                                        }
                                                                        else if (leg_.Group == "1")
                                                                        {
                                                                            finalBond.BoundType = "InBound";
                                                                        }

                                                                    }
                                                                    else if (!groupStatus && !group.Equals(leg_.Group))
                                                                    {
                                                                        finalBond.Legs.Add(leg_);
                                                                        //Added
                                                                        if (leg_.Group == "0")
                                                                        {
                                                                            finalBond.BoundType = "OutBound";
                                                                        }
                                                                        else if (leg_.Group == "1")
                                                                        {
                                                                            finalBond.BoundType = "InBound";
                                                                        }
                                                                    }
                                                                    else if (groupStatus && group.Equals(leg_.Group))
                                                                    {
                                                                        finalBond.Legs.Add(leg_);
                                                                        //Added
                                                                        if (leg_.Group == "0")
                                                                        {
                                                                            finalBond.BoundType = "OutBound";
                                                                        }
                                                                        else if (leg_.Group == "1")
                                                                        {
                                                                            finalBond.BoundType = "InBound";
                                                                        }
                                                                    }
                                                                }
                                                                seg.Bonds.Add(finalBond);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            seg.BondType = contractType_;
                                                            seg.Bonds.Add(bond);
                                                        }
                                                        if (seg.Fare.PaxFares != null && seg.Fare.PaxFares.Count > 0)
                                                        {
                                                            int tmfrom = 4;
                                                            int tmto = 0;
                                                            //cancelTime = CancellationCharges.GetCancellationTime();
                                                            //foreach (AirLine ar in cancelTime.AirLines)
                                                            //{
                                                            //    if (seg.Bonds[0].Legs[0].AirlineName == ar.Code)
                                                            //    {
                                                            //        foreach (AirlineClass ac in ar.AirlineClasses)
                                                            //        {
                                                            //            if (seg.Bonds[0].Legs[0].FareClassOfService == ac.Type)
                                                            //            {
                                                            //                //airCodeMatch = true;
                                                            //                tmfrom = Convert.ToInt32(ac.TimeFrom);
                                                            //                tmto = Convert.ToInt32(ac.TimeTo);
                                                            //            }
                                                            //        }
                                                            //    }
                                                            //}
                                                            //CheckFareRule(seg);

                                                            if (paxFare.CancelPenalty == 0)
                                                            {
                                                                paxFare.Refundable = false;
                                                                //seg.FareRule = "CAN-BEF " + tmfrom + "_" + tmto + ":" + seg.Fare.PaxFares[0].CancelPenalty + "|" + "CHG-BEF " + tmfrom + "_" + tmto + ":" + seg.Fare.PaxFares[0].ChangePenalty + "|" + "EMTFee-" + 250 + "|" + "Msg:As per as airline rules.";
                                                            }

                                                            else
                                                            {
                                                                //seg.FareRule = "CAN-BEF " + tmfrom + "_" + tmto + ":" + seg.Fare.PaxFares[0].CancelPenalty + "|" + "CHG-BEF " + tmfrom + "_" + tmto + ":" + seg.Fare.PaxFares[0].ChangePenalty + "|" + "EMTFee-" + 250;
                                                            }
                                                        }
                                                        seg.EngineID = "AirService.Engine.TravelPort";
                                                        seg.ItineraryKey = TPtransactionId;
                                                        seg.SearchId = Convert.ToString("EngineID");
                                                        seg.IsSpecial = "";// IsSpecial;
                                                        //if (EngineID == 115 || IsSpecial)
                                                        //{
                                                        //    seg.IsSpecial = true;
                                                        //}
                                                        //  seg.Bonds.Add(bond);
                                                        listOfSegment.Add(seg);
                                                        bond = new Bond();
                                                        bond.Legs = new List<GDSResModel.Leg>();
                                                        contractId++;
                                                    }
                                                }
                                            }
                                            #endregion airPricingSolution
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                XmlDocument pricingSolution = new XmlDocument();
                NewPricingSolutionValue = new StringBuilder();
                if (!string.IsNullOrEmpty(AirPricingSolutinForPNR))
                {
                    pricingSolution.LoadXml(AirPricingSolutinForPNR);
                }
                //pricingSolution.LoadXml(AirPricingSolutinForPNR.Replace("C11", "CNN"));
                bool InitialNodeStatus = true;
                int paxCount = 0;
                foreach (XmlNode airPricingSolutin in pricingSolution)
                {
                    if (airPricingSolutin.Name.Equals("air:AirPricingSolution", StringComparison.OrdinalIgnoreCase))
                    {
                        if (InitialNodeStatus)
                        {
                            NewPricingSolutionValue.Append(AirPricingSolutinForPNR.Substring(0, AirPricingSolutinForPNR.IndexOf(">") + 1));
                            InitialNodeStatus = false;
                        }
                        foreach (XmlNode airPricingChidNode in airPricingSolutin)
                        {
                            if (airPricingChidNode.Name.Equals("air:AirSegmentRef", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (TPAirSegment tpairSegment in listofTPSegment)
                                {

                                    if (airPricingChidNode.Attributes["Key"].Value.Equals(tpairSegment.AirSegment, StringComparison.OrdinalIgnoreCase))
                                    {
                                        NewPricingSolutionValue.Append(tpairSegment.AirSegmentDetail);
                                    }

                                }
                            }
                            else if (airPricingChidNode.Name.Equals("air:AirPricingInfo", StringComparison.OrdinalIgnoreCase))
                            {
                                NewPricingSolutionValue.Append(airPricingChidNode.OuterXml.Substring(0, airPricingChidNode.OuterXml.IndexOf(">") + 1));
                                foreach (XmlNode airPricingInfo in airPricingChidNode)
                                {
                                    if (airPricingInfo.Name.Equals("air:PassengerType", StringComparison.OrdinalIgnoreCase))
                                    {
                                        switch (airPricingInfo.Attributes["Code"].Value)
                                        {
                                            case "ADT":
                                                NewPricingSolutionValue.Append("<air:PassengerType BookingTravelerRef='" + paxCount + "' Code='ADT' Age='30'/>");
                                                paxCount++;
                                                break;
                                            case "CNN":
                                                NewPricingSolutionValue.Append("<air:PassengerType BookingTravelerRef='" + paxCount + "' Code='CNN' Age='11'/>");
                                                paxCount++;
                                                break;
                                            case "C11":
                                                NewPricingSolutionValue.Append("<air:PassengerType BookingTravelerRef='" + paxCount + "' Code='CNN' Age='11'/>");
                                                paxCount++;
                                                break;
                                            case "CHD":
                                                NewPricingSolutionValue.Append("<air:PassengerType BookingTravelerRef='" + paxCount + "' Code='CHD' Age='11'/>");
                                                paxCount++;
                                                break;
                                            case "INF":
                                                NewPricingSolutionValue.Append("<air:PassengerType BookingTravelerRef='" + paxCount + "' Code='INF' Age='1'/>");
                                                paxCount++;
                                                break;

                                        }
                                    }
                                    else
                                    {
                                        NewPricingSolutionValue.Append(airPricingInfo.OuterXml);
                                    }
                                }
                                NewPricingSolutionValue.Append("</air:AirPricingInfo>");
                            }
                            else
                            {
                                NewPricingSolutionValue.Append(airPricingChidNode.OuterXml);
                            }
                        }
                    }
                    NewPricingSolutionValue.Append("</air:AirPricingSolution>");
                }
                if (listOfSegment != null && listOfSegment.Count > 0)
                {
                    bool IsSpecial = false;
                    string AccountCode = "";
                    bool PermittedCarriers = false;
                    // if (EngineID == 115)
                    if (IsSpecial && PermittedCarriers)
                    {
                        listOfSegment[0].PricingSolutionValue = NewPricingSolutionValue.ToString().Replace("<air:BaggageAllowances", "<air:AirPricingModifiers AccountCodeFaresOnly='true'><air:AccountCodes><com:AccountCode Code='" + AccountCode + "'/></air:AccountCodes></air:AirPricingModifiers><air:BaggageAllowances").Replace("common_v46_0:", "com:");
                    }
                    else
                    {
                        listOfSegment[0].PricingSolutionValue = NewPricingSolutionValue.ToString();
                    }
                }
            }
            catch (SystemException ex_)
            {
            }
            return listOfSegment;
        }
        public List<GDSResModel.Segment> ParseLowFareSearchRsp2(string lowFareResponse, string contractType_, DateTime depDate)
        {
            List<GDSResModel.Segment> listOfSegment = new List<GDSResModel.Segment>();
            //ArrayList listofTPSegment = null;
            ArrayList listOfBound = new ArrayList();
            int contractId = 1;
            string TPtransactionId = string.Empty;
            string outBoundGroup = string.Empty;
            string inBoundGroup = string.Empty;
            int journeyIndex = 0;
            bool IsFlex = true;
            string _journeyTime = string.Empty;
            //List<Bond> listOfBound = new List<Bond>();
            GDSResModel.Segment seg = null;
            bool airSegstatus = false;
            XmlDocument flightDetailsList = new XmlDocument();
            XmlDocument FareInfoList = new XmlDocument();
            XmlDocument BrandList = null;
            string fareRoues = string.Empty;
            GDSResModel.Fare fare = null;
            Dictionary<string, string> baggageDetais = new Dictionary<string, string>();
            Dictionary<string, string> fareRuleInfo = new Dictionary<string, string>();

            GDSResModel.Leg leg = null;
            GDSResModel.PaxFare paxFare = new GDSResModel.PaxFare();
            GDSResModel.FareDetail fareDetails = new GDSResModel.FareDetail();
            bool IsDomestic = true;
            string segmentid = string.Empty;
            string flightnumber = string.Empty;
            //string filePath = "C:\\Users\\Jet\\Desktop\\1072024_1.xml";
            //string lowFareResponse = File.ReadAllText(filePath);
            //List<Segment> data =  GetSegmentList(lowFareResponse);
            XmlDocument doc = new XmlDocument();
            XmlDocument airSegmentList = new XmlDocument();
            if (!string.IsNullOrEmpty(lowFareResponse))
            {
                doc.LoadXml(lowFareResponse);
            }
            //doc.LoadXml(lowFareResponse.Replace("C11", "CNN"));
            GDSResModel.Bond bond = new GDSResModel.Bond();
            bond.Legs = new List<GDSResModel.Leg>();
            //cancelTime = new FareCancellationTime();
            foreach (XmlNode rootNode in doc) //LOOP ON ROOT GOT 1 SOAPBODY 
            {
                if (rootNode.Name.Equals("SOAP:Envelope", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (XmlNode root in rootNode.ChildNodes) //LOOP FOR SOAPBODY(SOAPbODY)
                    {
                        if (root.Name.Equals("SOAP:Body", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (XmlNode lowFareSearchRes in root)// LOOP FOR SOAPBODY GOT 1 NODE AS LOWFARESEARCHRSP (LOWFARESEARCHRSP)
                            {
                                if (lowFareSearchRes.Name.Equals("air:LowFareSearchRsp", StringComparison.OrdinalIgnoreCase))
                                {
                                    TPtransactionId = lowFareSearchRes.Attributes["TransactionId"].InnerText;
                                    foreach (XmlNode airPricingSolution in lowFareSearchRes) // LOOP HERE FOR lOWFARESEARCHrSP
                                    {
                                        if (airPricingSolution.Name.Equals("air:FlightDetailsList", StringComparison.OrdinalIgnoreCase))
                                        {
                                            flightDetailsList = new XmlDocument();
                                            flightDetailsList.LoadXml(airPricingSolution.OuterXml);
                                        }
                                        if (airPricingSolution.Name.Equals("air:RouteList", StringComparison.OrdinalIgnoreCase))
                                        {
                                            foreach (XmlNode routelist in airPricingSolution)
                                            {
                                                if (routelist.Name.Equals("air:Route", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    foreach (XmlNode airroute in routelist)
                                                    {

                                                        if (airroute.Name.Equals("air:Leg", StringComparison.OrdinalIgnoreCase) && airroute.Attributes["Group"].Value.Equals("0"))//&& RQ.SearchDetails.Count == 1
                                                        {
                                                            outBoundGroup = airroute.Attributes["Group"].Value;
                                                        }
                                                        else if (airroute.Name.Equals("air:Leg", StringComparison.OrdinalIgnoreCase))// && RQ.SearchDetails.Count == 2 
                                                        {
                                                            if (airroute.Attributes["Group"].Value.Equals("0"))
                                                            {
                                                                outBoundGroup = airroute.Attributes["Group"].Value;
                                                            }
                                                            else if (airroute.Attributes["Group"].Value.Equals("1"))
                                                            {
                                                                inBoundGroup = airroute.Attributes["Group"].Value;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (airPricingSolution.Name.Equals("air:FareInfoList", StringComparison.OrdinalIgnoreCase))
                                        {
                                            FareInfoList = new XmlDocument();
                                            FareInfoList.LoadXml(airPricingSolution.OuterXml);
                                        }
                                        #region AirPricingSolution
                                        if (airPricingSolution.Name.Equals("air:AirPricingSolution", StringComparison.OrdinalIgnoreCase))
                                        {
                                            fare = new GDSResModel.Fare();
                                            fareRoues = string.Empty;
                                            baggageDetais = new Dictionary<string, string>();
                                            fareRuleInfo = new Dictionary<string, string>();
                                            decimal TMarkup = 0;
                                            if (airPricingSolution.Attributes["TotalPrice"].Value.Contains("INR"))
                                            {
                                                fare.TotalFareWithOutMarkUp = Convert.ToDecimal(airPricingSolution.Attributes["TotalPrice"].Value.Remove(0, 3));
                                                if (fare.TotalFareWithOutMarkUp == 55057)
                                                {

                                                }
                                            }
                                            else
                                            {
                                                fare.TotalFareWithOutMarkUp = Convert.ToDecimal(airPricingSolution.Attributes["ApproximateTotalPrice"].Value.Remove(0, 3));
                                            }
                                            if (airPricingSolution.Attributes["BasePrice"].Value.Contains("INR"))
                                            {
                                                fare.BasicFare = Convert.ToDecimal(airPricingSolution.Attributes["BasePrice"].Value.Remove(0, 3));
                                            }
                                            else
                                            {
                                                fare.BasicFare = Convert.ToDecimal(airPricingSolution.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                            }

                                            if (airPricingSolution.Attributes["Taxes"].Value.Contains("INR"))
                                            {
                                                //TMarkup = GetMarkup(Convert.ToDecimal(airPricingSolution.Attributes["Taxes"].Value.Remove(0, 3)), Convert.ToDecimal(airPricingSolution.Attributes["Taxes"].Value.Remove(0, 3)), Convert.ToDecimal(airPricingSolution.Attributes["Taxes"].Value.Remove(0, 3)));
                                                fare.TotalTaxWithOutMarkUp = Convert.ToDecimal(airPricingSolution.Attributes["Taxes"].Value.Remove(0, 3)) + TMarkup;

                                            }
                                            else
                                            {
                                                //TMarkup = GetMarkup(Convert.ToDecimal(airPricingSolution.Attributes["Taxes"].Value.Remove(0, 3)), Convert.ToDecimal(airPricingSolution.Attributes["Taxes"].Value.Remove(0, 3)), Convert.ToDecimal(airPricingSolution.Attributes["Taxes"].Value.Remove(0, 3)));
                                                fare.TotalTaxWithOutMarkUp = Convert.ToDecimal(airPricingSolution.Attributes["ApproximateTaxes"].Value.Remove(0, 3)) + TMarkup;
                                            }
                                            fare.PaxFares = new List<GDSResModel.PaxFare>();
                                            bool flag = true;
                                            foreach (XmlNode lowfarepric in airPricingSolution)
                                            {
                                                switch (lowfarepric.Name)
                                                {
                                                    case "air:Journey":
                                                        flightnumber = string.Empty;
                                                        bond = new GDSResModel.Bond();
                                                        bond.Legs = new List<GDSResModel.Leg>();
                                                        int stop = 0;
                                                        journeyIndex = 0;
                                                        airSegstatus = false;
                                                        _journeyTime = null;
                                                        foreach (XmlNode airSegmentKeys in lowfarepric)
                                                        {
                                                            if (airSegmentKeys.Name.Equals("air:AirSegmentRef", StringComparison.OrdinalIgnoreCase)) // connected
                                                            {
                                                                leg = new GDSResModel.Leg();
                                                                if (airSegmentList != null && !string.IsNullOrEmpty(airSegmentList.InnerXml))
                                                                {
                                                                    foreach (XmlNode airSegmentlist in airSegmentList)
                                                                    {
                                                                        if (airSegmentlist.Name.Equals("air:AirSegmentList", StringComparison.OrdinalIgnoreCase))
                                                                        {
                                                                            foreach (XmlNode airSegment in airSegmentlist)
                                                                            {
                                                                                if (airSegment.Name.Equals("air:AirSegment", StringComparison.OrdinalIgnoreCase))
                                                                                {
                                                                                    //connected check <air:AirSegmentRef Key="eAo/mASqWDKAFoxI3AAAAA=="/>
                                                                                    if (airSegment.Attributes["Key"].Value.Equals(airSegmentKeys.Attributes["Key"].InnerText, StringComparison.Ordinal))
                                                                                    {
                                                                                        if (!airSegstatus && IsFlex)
                                                                                        {
                                                                                            //journeyIndex = depDate.Subtract(Convert.ToDateTime(airSegment.Attributes["DepartureTime"].Value).Date).Days;
                                                                                            //journeyIndex = depDate.DayOfYear - (Convert.ToDateTime(airSegment.Attributes["DepartureTime"].Value.Split('+')[0])).DayOfYear;
                                                                                            airSegstatus = true;
                                                                                        }
                                                                                        if (airSegment.Attributes["Group"].Value.Equals(outBoundGroup, StringComparison.OrdinalIgnoreCase))
                                                                                        {
                                                                                            leg.BoundType = "OutBound";
                                                                                            leg.Group = outBoundGroup;
                                                                                            bond.BoundType = "OutBound";
                                                                                        }
                                                                                        else if (airSegment.Attributes["Group"].Value.Equals(inBoundGroup, StringComparison.OrdinalIgnoreCase))
                                                                                        {
                                                                                            leg.BoundType = "InBound";
                                                                                            leg.Group = inBoundGroup;
                                                                                            bond.BoundType = "InBound";
                                                                                        }

                                                                                        if (airSegment.Attributes["NumberOfStops"] != null)
                                                                                        {
                                                                                            leg.NumberOfStops = airSegment.Attributes["NumberOfStops"].Value;
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            leg.NumberOfStops = "0";
                                                                                        }
                                                                                        leg.FlightNumber = airSegment.Attributes["FlightNumber"].Value;
                                                                                        leg.AirlineName = airSegment.Attributes["Carrier"].Value;
                                                                                        leg.CarrierCode = airSegment.Attributes["Carrier"].Value;
                                                                                        leg.Origin = airSegment.Attributes["Origin"].Value;
                                                                                        leg.Destination = airSegment.Attributes["Destination"].Value;
                                                                                        leg.DepartureDate = airSegment.Attributes["DepartureTime"].Value.Split('T')[0];
                                                                                        leg.DepartureTime = airSegment.Attributes["DepartureTime"].Value; // airSegment.Attributes["DepartureTime"].Value.Split('T')[1];
                                                                                        leg.ArrivalDate = airSegment.Attributes["ArrivalTime"].Value.Split('T')[0];
                                                                                        leg.ArrivalTime = airSegment.Attributes["ArrivalTime"].Value; //airSegment.Attributes["ArrivalTime"].Value.Split('T')[1];
                                                                                        leg._ArrivalDate = airSegment.Attributes["ArrivalTime"].Value;
                                                                                        leg._DepartureDate = Convert.ToDateTime(airSegment.Attributes["DepartureTime"].Value).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
                                                                                        leg._Distance = airSegment.Attributes["Distance"].Value;
                                                                                        leg._Equipment = airSegment.Attributes["Equipment"].Value;
                                                                                        leg._AvailabilityDisplayType = airSegment.Attributes["AvailabilityDisplayType"].Value;
                                                                                        leg._AvailabilitySource = airSegment.Attributes["AvailabilitySource"].Value;
                                                                                        leg.Duration = airSegment.Attributes["FlightTime"].Value;
                                                                                        leg.AircraftCode = airSegmentKeys.Attributes["Key"].InnerText;
                                                                                        if (flightnumber == "")
                                                                                        {
                                                                                            flightnumber += airSegment.Attributes["FlightNumber"].Value;
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            flightnumber += "@" + airSegment.Attributes["FlightNumber"].Value;
                                                                                        }
                                                                                        if(flightnumber=="573" || flightnumber=="545")
                                                                                        {

                                                                                        }
                                                                                        switch (leg.AirlineName)
                                                                                        {
                                                                                            case "AI":
                                                                                                leg.FlightName = "AirIndia";
                                                                                                break;
                                                                                            case "9W":
                                                                                                leg.FlightName = "JetAirWays";
                                                                                                break;
                                                                                            case "UK":
                                                                                                leg.FlightName = "Vistara";
                                                                                                break;
                                                                                            case "OD":
                                                                                                leg.FlightName = "MALINDO AIRWAYS";
                                                                                                break;
                                                                                            case "TG":
                                                                                                leg.FlightName = "Thai Airways International";
                                                                                                break;
                                                                                            case "SV":
                                                                                                leg.FlightName = "Saudia";
                                                                                                break;
                                                                                            case "UL":
                                                                                                leg.FlightName = "SriLankan Airlines";
                                                                                                break;
                                                                                            default:
                                                                                                try
                                                                                                {
                                                                                                    leg.FlightName = "";// Utility.FileChangeMonitor.AirlineNames[leg.AirlineName.Trim()];
                                                                                                }
                                                                                                catch (SystemException sex_) { leg.FlightName = leg.AirlineName.Trim(); }
                                                                                                break;
                                                                                        }
                                                                                        foreach (XmlNode airSegmentChild in airSegment)
                                                                                        {
                                                                                            switch (airSegmentChild.Name)
                                                                                            {
                                                                                                case "air:AirAvailInfo":
                                                                                                    leg.ProviderCode = airSegmentChild.Attributes["ProviderCode"].Value;
                                                                                                    break;
                                                                                                case "air:FlightDetailsRef":
                                                                                                    leg.FlightDetailRefKey = airSegmentChild.Attributes["Key"].Value;
                                                                                                    if (flightDetailsList != null && !string.IsNullOrEmpty(flightDetailsList.InnerXml))
                                                                                                    {
                                                                                                        foreach (XmlNode flightDetaillist_ in flightDetailsList)
                                                                                                        {
                                                                                                            foreach (XmlNode flightDetails in flightDetaillist_)
                                                                                                            {
                                                                                                                if (flightDetails.Name.Equals("air:FlightDetails", StringComparison.OrdinalIgnoreCase) && flightDetails.Attributes["Key"].Value.Equals(airSegmentChild.Attributes["Key"].Value, StringComparison.Ordinal))
                                                                                                                {
                                                                                                                    if ((flightDetails.Attributes["OriginTerminal"]) != null)
                                                                                                                    {
                                                                                                                        leg.DepartureTerminal = flightDetails.Attributes["OriginTerminal"].Value;
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {
                                                                                                                        leg.DepartureTerminal = string.Empty;
                                                                                                                    }
                                                                                                                    if ((flightDetails.Attributes["DestinationTerminal"]) != null)
                                                                                                                    {
                                                                                                                        leg.ArrivalTerminal = flightDetails.Attributes["DestinationTerminal"].Value;
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {
                                                                                                                        leg.ArrivalTerminal = string.Empty;
                                                                                                                    }
                                                                                                                    _journeyTime = lowfarepric.Attributes["TravelTime"].Value;
                                                                                                                    if (!string.IsNullOrWhiteSpace(_journeyTime) && string.IsNullOrWhiteSpace(bond.JourneyTime))
                                                                                                                    {
                                                                                                                        bond.JourneyTime = (Convert.ToInt32(_journeyTime.Substring(1, 1)) * 24 * 60 + Convert.ToInt32((_journeyTime.Substring(_journeyTime.IndexOf('T') + 1, _journeyTime.IndexOf('H') - _journeyTime.IndexOf('T') - 1))) * 60 + Convert.ToInt32(_journeyTime.Substring(_journeyTime.IndexOf('H') + 1, _journeyTime.LastIndexOf('M') - _journeyTime.LastIndexOf('H') - 1))).ToString();
                                                                                                                    }
                                                                                                                    break;
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                    break;
                                                                                            }
                                                                                        }
                                                                                        bond.Legs.Add(leg);
                                                                                        stop++;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    bond.FlightNumber = flightnumber;
                                                                }
                                                            }
                                                        }
                                                        listOfBound.Add(bond);
                                                        break;
                                                    case "air:AirPricingInfo":
                                                        if (flag == false)
                                                        {
                                                            continue;
                                                        }
                                                        segmentid = string.Empty;
                                                        paxFare = new GDSResModel.PaxFare();
                                                        paxFare.Fare = new List<GDSResModel.FareDetail>();
                                                        if (lowfarepric.Attributes["Refundable"] != null)
                                                        {
                                                            paxFare.Refundable = bool.Parse(lowfarepric.Attributes["Refundable"].Value);
                                                        }
                                                        flag = false;
                                                        foreach (XmlNode airPricingInfo in lowfarepric)
                                                        {
                                                            if (airPricingInfo.Name.Equals("air:BookingInfo", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                if (airPricingInfo.Attributes["CabinClass"] != null)
                                                                {
                                                                    paxFare.FareBasisCode = airPricingInfo.Attributes["CabinClass"].Value;
                                                                }
                                                                if (airPricingInfo.Attributes["SegmentRef"] != null)
                                                                {
                                                                    if (segmentid == "")
                                                                    {
                                                                        segmentid += airPricingInfo.Attributes["SegmentRef"].Value;
                                                                    }
                                                                    else
                                                                    {
                                                                        segmentid += "@" + airPricingInfo.Attributes["SegmentRef"].Value;
                                                                    }
                                                                }
                                                            }
                                                            if (airPricingInfo.Name.Equals("air:TaxInfo", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                if (airPricingInfo.Attributes["Category"] != null && airPricingInfo.Attributes["Amount"] != null)
                                                                {
                                                                    fareDetails = new GDSResModel.FareDetail();
                                                                    fareDetails.Amount = decimal.Parse(airPricingInfo.Attributes["Amount"].Value.Remove(0, 3));
                                                                    switch (airPricingInfo.Attributes["Category"].Value)
                                                                    {
                                                                        case "IN":
                                                                            fareDetails.ChargeCode = "UDF";
                                                                            fareDetails.ChargeDetail = "USER DEVELOPMENT FEE";
                                                                            break;
                                                                        case "JN":
                                                                            fareDetails.ChargeCode = "ST";
                                                                            fareDetails.ChargeDetail = "SERVICE TAX";
                                                                            break;
                                                                        case "WO":
                                                                            fareDetails.ChargeCode = "PSF";
                                                                            fareDetails.ChargeDetail = "PASSENGER SERVICE FEE";
                                                                            break;
                                                                        case "YM":
                                                                            fareDetails.ChargeCode = "ADF";
                                                                            fareDetails.ChargeDetail = "AIRPORT DEVELOPMENT FEE";
                                                                            break;
                                                                        case "YQ":
                                                                            fareDetails.ChargeCode = "YQ";
                                                                            fareDetails.ChargeDetail = "Fuel Expenses";
                                                                            break;
                                                                        case "YR":
                                                                            fareDetails.ChargeCode = "YR";
                                                                            fareDetails.ChargeDetail = "Fuel Expenses";
                                                                            break;
                                                                        default:
                                                                            fareDetails.ChargeCode = airPricingInfo.Attributes["Category"].Value;
                                                                            fareDetails.ChargeDetail = airPricingInfo.Attributes["Category"].Value;
                                                                            break;
                                                                    }
                                                                    paxFare.Fare.Add(fareDetails);
                                                                }
                                                            }


                                                            if (airPricingInfo.Name.Equals("air:PassengerType", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                switch (airPricingInfo.Attributes["Code"].Value)
                                                                {
                                                                    case "ADT":
                                                                        if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        else
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                        paxFare.PaxType = GDSResModel.PAXTYPE.ADT;
                                                                        //if (AdultMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                        //{
                                                                        //    paxFare.TotalTax += AdultMarkUp;
                                                                        //    paxFare.Fare[0].Amount += AdultMarkUp;
                                                                        //}
                                                                        //if (MarkUpType == MarkUP.Percentage && AdultMarkUp != 0)
                                                                        //{
                                                                        //    paxFare.TotalTax += (paxFare.TotalTax * (AdultMarkUp / 100));
                                                                        //    paxFare.Fare[0].Amount += (paxFare.TotalTax * (AdultMarkUp / 100));
                                                                        //}
                                                                        break;
                                                                    case "CNN":
                                                                        if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        else
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                        paxFare.PaxType = GDSResModel.PAXTYPE.CHD;
                                                                        //if (ChildMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                        //{
                                                                        //    paxFare.TotalTax += ChildMarkUp;
                                                                        //    paxFare.Fare[0].Amount += ChildMarkUp;
                                                                        //}
                                                                        //if (MarkUpType == MarkUP.Percentage && ChildMarkUp != 0)
                                                                        //{
                                                                        //    paxFare.TotalTax += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                        //    paxFare.Fare[0].Amount += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                        //}
                                                                        break;
                                                                    case "C11":
                                                                        if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        else
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                        paxFare.PaxType = GDSResModel.PAXTYPE.CHD;
                                                                        //if (ChildMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                        //{
                                                                        //    paxFare.TotalTax += ChildMarkUp;
                                                                        //    paxFare.Fare[0].Amount += ChildMarkUp;
                                                                        //}
                                                                        //if (MarkUpType == MarkUP.Percentage && ChildMarkUp != 0)
                                                                        //{
                                                                        //    paxFare.TotalTax += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                        //    paxFare.Fare[0].Amount += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                        //}
                                                                        break;
                                                                    case "CHD":
                                                                        if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        else
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                        paxFare.PaxType = GDSResModel.PAXTYPE.CHD;
                                                                        //if (ChildMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                        //{
                                                                        //    paxFare.TotalTax += ChildMarkUp;
                                                                        //    paxFare.Fare[0].Amount += ChildMarkUp;
                                                                        //}
                                                                        //if (MarkUpType == MarkUP.Percentage && ChildMarkUp != 0)
                                                                        //{
                                                                        //    paxFare.TotalTax += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                        //    paxFare.Fare[0].Amount += (paxFare.TotalTax * (ChildMarkUp / 100));
                                                                        //}
                                                                        break;
                                                                    case "INF":
                                                                        if (lowfarepric.Attributes["BasePrice"].Value.Contains("INR"))
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["BasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        else
                                                                        {
                                                                            paxFare.BasicFare = Convert.ToDecimal(lowfarepric.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                        }
                                                                        paxFare.TotalTax = Convert.ToDecimal(lowfarepric.Attributes["Taxes"].Value.Remove(0, 3));
                                                                        paxFare.PaxType = GDSResModel.PAXTYPE.INF;
                                                                        //if (InfantMarkUp != 0 && MarkUpType == MarkUP.Flat)
                                                                        //{
                                                                        //    paxFare.TotalTax += InfantMarkUp;
                                                                        //    paxFare.Fare[0].Amount += InfantMarkUp;
                                                                        //}
                                                                        //if (MarkUpType == MarkUP.Percentage && InfantMarkUp != 0)
                                                                        //{
                                                                        //    paxFare.TotalTax += (paxFare.TotalTax * (InfantMarkUp / 100));
                                                                        //    paxFare.Fare[0].Amount += (paxFare.TotalTax * (InfantMarkUp / 100));
                                                                        //}
                                                                        break;
                                                                }
                                                            }
                                                            if (airPricingInfo.Name.Equals("air:ChangePenalty", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                foreach (XmlNode changePenalty in airPricingInfo)
                                                                {
                                                                    if (changePenalty.Name.Equals("air:Amount", StringComparison.OrdinalIgnoreCase))
                                                                    {
                                                                        paxFare.ChangePenalty = Convert.ToDecimal(changePenalty.InnerText.Remove(0, 3)) * ((GDSResModel.Bond)listOfBound[0]).Legs.Count;
                                                                    }
                                                                }
                                                            }
                                                            if (airPricingInfo.Name.Equals("air:CancelPenalty", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                foreach (XmlNode cancelPenalty in airPricingInfo)
                                                                {
                                                                    if (cancelPenalty.Name.Equals("air:Amount", StringComparison.OrdinalIgnoreCase))
                                                                    {
                                                                        paxFare.CancelPenalty = Convert.ToDecimal(cancelPenalty.InnerText.Remove(0, 3)) * ((GDSResModel.Bond)listOfBound[0]).Legs.Count;
                                                                        if (paxFare.CancelPenalty == 0)
                                                                        {
                                                                            paxFare.Refundable = false;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            //   if (airPricingInfo.Name.Equals("air:MiniFareRules", StringComparison.OrdinalIgnoreCase))
                                                            if (airPricingInfo.Name.Equals("air:FareRulesFilter", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                foreach (XmlNode miniFareRules in airPricingInfo)
                                                                {
                                                                    if (miniFareRules.Name.Equals("air:Refundability", StringComparison.OrdinalIgnoreCase))
                                                                    {
                                                                        if (miniFareRules.Attributes["Value"] != null)
                                                                        {
                                                                            if (miniFareRules.Attributes["Value"].Value.Equals("NonRefundable", StringComparison.OrdinalIgnoreCase))
                                                                            {
                                                                                paxFare.Refundable = false;
                                                                            }
                                                                            if (string.IsNullOrWhiteSpace(fareRoues))
                                                                            {
                                                                                fareRoues = miniFareRules.Attributes["Value"].Value;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            if (airPricingInfo.Name.Equals("air:FareInfoRef", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                if (FareInfoList != null && !string.IsNullOrEmpty(FareInfoList.InnerXml))
                                                                {
                                                                    foreach (XmlNode fareInfoList in FareInfoList)
                                                                    {
                                                                        if (fareInfoList.Name.Equals("air:FareInfoList", StringComparison.OrdinalIgnoreCase))
                                                                        {
                                                                            foreach (XmlNode fareInfo in fareInfoList)
                                                                            {
                                                                                if (fareInfo.Attributes["Key"].Value.Equals(airPricingInfo.Attributes["Key"].Value, StringComparison.Ordinal))
                                                                                {
                                                                                    //GetBrandDetails
                                                                                    string _BrandDesc = string.Empty;
                                                                                    try
                                                                                    {
                                                                                        #region GetBrandDetails
                                                                                        foreach (XmlNode BrandInfo in fareInfo)
                                                                                        {
                                                                                            if (BrandInfo.Name.Equals("air:Brand", StringComparison.OrdinalIgnoreCase))
                                                                                            {
                                                                                                if (BrandInfo.Attributes["BrandID"] != null)
                                                                                                {
                                                                                                    //string _BrandId = baggageAllowance.Attributes["BrandID"].Value;
                                                                                                    if (BrandList != null)
                                                                                                    {
                                                                                                        if (BrandList != null && !string.IsNullOrEmpty(BrandList.InnerXml))
                                                                                                        {
                                                                                                            foreach (XmlNode brandLists in BrandList)
                                                                                                            {
                                                                                                                if (brandLists.Name.Equals("air:BrandList", StringComparison.OrdinalIgnoreCase))
                                                                                                                {
                                                                                                                    foreach (XmlNode brand in brandLists)
                                                                                                                    {
                                                                                                                        if (brand.Attributes["BrandID"].Value.Equals(BrandInfo.Attributes["BrandID"].Value, StringComparison.OrdinalIgnoreCase))
                                                                                                                        {
                                                                                                                            foreach (XmlNode brandText in brand)
                                                                                                                            {
                                                                                                                                if (brandText.Name.Equals("air:Title", StringComparison.OrdinalIgnoreCase) && brandText.Attributes["Type"].Value.Equals("External", StringComparison.OrdinalIgnoreCase))
                                                                                                                                {
                                                                                                                                    _BrandDesc = brandText.InnerText;
                                                                                                                                    break;
                                                                                                                                }
                                                                                                                                //if (brandText.Name.Equals("air:Text", StringComparison.OrdinalIgnoreCase) && brandText.Attributes["Type"].Value.Equals("MarketingAgent", StringComparison.OrdinalIgnoreCase))
                                                                                                                                //{
                                                                                                                                //    _BrandDesc = brandText.InnerText;

                                                                                                                                //}
                                                                                                                            }

                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                                break;
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        foreach (XmlNode brandList in lowFareSearchRes)
                                                                                                        {
                                                                                                            if (brandList.Name.Equals("air:BrandList", StringComparison.OrdinalIgnoreCase))
                                                                                                            {
                                                                                                                BrandList = new XmlDocument();
                                                                                                                BrandList.LoadXml(brandList.OuterXml);
                                                                                                                if (BrandList != null && !string.IsNullOrEmpty(BrandList.InnerXml))
                                                                                                                {
                                                                                                                    foreach (XmlNode brandLists in BrandList)
                                                                                                                    {
                                                                                                                        if (brandLists.Name.Equals("air:BrandList", StringComparison.OrdinalIgnoreCase))
                                                                                                                        {
                                                                                                                            foreach (XmlNode brand in brandLists)
                                                                                                                            {
                                                                                                                                if (brand.Attributes["BrandID"].Value.Equals(BrandInfo.Attributes["BrandID"].Value, StringComparison.OrdinalIgnoreCase))
                                                                                                                                {
                                                                                                                                    foreach (XmlNode brandText in brand)
                                                                                                                                    {
                                                                                                                                        //if (brandText.Name.Equals("air:Text", StringComparison.OrdinalIgnoreCase) && brandText.Attributes["Type"].Value.Equals("MarketingAgent", StringComparison.OrdinalIgnoreCase))
                                                                                                                                        //{
                                                                                                                                        //    _BrandDesc = brandText.InnerText;

                                                                                                                                        //}
                                                                                                                                        if (brandText.Name.Equals("air:Title", StringComparison.OrdinalIgnoreCase) && brandText.Attributes["Type"].Value.Equals("External", StringComparison.OrdinalIgnoreCase))
                                                                                                                                        {
                                                                                                                                            _BrandDesc = brandText.InnerText;
                                                                                                                                            break;
                                                                                                                                        }
                                                                                                                                    }

                                                                                                                                }
                                                                                                                                //break;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        break;
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        #endregion
                                                                                    }
                                                                                    catch
                                                                                    {

                                                                                    }
                                                                                    //air:BaggageAllowance>
                                                                                    foreach (XmlNode baggageAllowance in fareInfo)
                                                                                    {
                                                                                        if (baggageAllowance.Name.Equals("air:BaggageAllowance", StringComparison.OrdinalIgnoreCase))
                                                                                        {
                                                                                            foreach (XmlNode maxWeight in baggageAllowance)
                                                                                            {
                                                                                                if (maxWeight.Name.Equals("air:MaxWeight", StringComparison.OrdinalIgnoreCase))
                                                                                                {
                                                                                                    if (maxWeight.Attributes["Value"] != null)
                                                                                                    {
                                                                                                        paxFare.BaggageWeight = maxWeight.Attributes["Value"].Value;
                                                                                                        paxFare.BaggageUnit = "KG";// maxWeight.Attributes["Unit"].Value;
                                                                                                        if (!baggageDetais.ContainsKey(fareInfo.Attributes["Key"].Value))
                                                                                                            baggageDetais.Add(fareInfo.Attributes["Key"].Value, maxWeight.Attributes["Value"].Value + "|" + "KG|" + _BrandDesc);

                                                                                                    }
                                                                                                    break;
                                                                                                }
                                                                                                if (maxWeight.Name.Equals("air:NumberOfPieces", StringComparison.OrdinalIgnoreCase))
                                                                                                {
                                                                                                    paxFare.BaggageWeight = maxWeight.InnerText;
                                                                                                    paxFare.BaggageUnit = "PC";
                                                                                                    if (!baggageDetais.ContainsKey(fareInfo.Attributes["Key"].Value))
                                                                                                        baggageDetais.Add(fareInfo.Attributes["Key"].Value, maxWeight.InnerText + "|" + "PC|" + _BrandDesc);
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        if (baggageAllowance.Name.Equals("air:FareRuleKey", StringComparison.OrdinalIgnoreCase))
                                                                                        {
                                                                                            paxFare.FareInfoKey = baggageAllowance.Attributes["FareInfoRef"].Value;
                                                                                            paxFare.FareInfoValue = baggageAllowance.InnerText;
                                                                                            if (!fareRuleInfo.ContainsKey(baggageAllowance.Attributes["FareInfoRef"].Value))
                                                                                                fareRuleInfo.Add(baggageAllowance.Attributes["FareInfoRef"].Value, baggageAllowance.InnerText);
                                                                                        }
                                                                                    }
                                                                                    break;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            if (airPricingInfo.Name.Equals("air:BookingInfo", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                foreach (GDSResModel.Bond bond_ in listOfBound)
                                                                {
                                                                    foreach (GDSResModel.Leg leg_ in bond_.Legs)
                                                                    {
                                                                        if (leg_.AircraftCode.Equals(airPricingInfo.Attributes["SegmentRef"].Value, StringComparison.Ordinal))
                                                                        {
                                                                            leg_.FareClassOfService = airPricingInfo.Attributes["BookingCode"].Value;
                                                                            leg_.AvailableSeat = airPricingInfo.Attributes["BookingCount"].Value;
                                                                            leg_.Cabin = airPricingInfo.Attributes["CabinClass"].Value;
                                                                            leg_.FareRulesKey = airPricingInfo.Attributes["FareInfoRef"].Value;
                                                                            leg_.FareRulesValue = fareRuleInfo[airPricingInfo.Attributes["FareInfoRef"].Value];
                                                                            if (FareInfoList != null && !string.IsNullOrEmpty(FareInfoList.InnerXml))
                                                                            {
                                                                                foreach (XmlNode fareInfoList in FareInfoList)
                                                                                {
                                                                                    if (fareInfoList.Name.Equals("air:FareInfoList", StringComparison.OrdinalIgnoreCase))
                                                                                    {
                                                                                        foreach (XmlNode fareInfo in fareInfoList)
                                                                                        {
                                                                                            if (fareInfo.Attributes["Key"].Value.Equals(leg_.FareRulesKey, StringComparison.Ordinal))
                                                                                            {
                                                                                                leg_._FareBasisCodeforAirpriceHit = fareInfo.Attributes["FareBasis"].Value;
                                                                                                break;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }


                                                                            if (baggageDetais != null && baggageDetais.ContainsKey(airPricingInfo.Attributes["FareInfoRef"].Value))
                                                                            {
                                                                                if (baggageDetais[airPricingInfo.Attributes["FareInfoRef"].Value].Split('|').Length == 3)
                                                                                {
                                                                                    leg_.BaggageWeight = baggageDetais[airPricingInfo.Attributes["FareInfoRef"].Value].Split('|')[0];
                                                                                    leg_.BaggageUnit = baggageDetais[airPricingInfo.Attributes["FareInfoRef"].Value].Split('|')[1];
                                                                                    leg_.Branddesc = baggageDetais[airPricingInfo.Attributes["FareInfoRef"].Value].Split('|')[2].Split('\n')[0];
                                                                                    if (!IsDomestic)
                                                                                        leg_.Remarks = baggageDetais[airPricingInfo.Attributes["FareInfoRef"].Value].Split('|')[2];
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                leg_.BaggageWeight = "15";
                                                                                leg_.BaggageUnit = "KG";
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        if (paxFare.TotalFare == 0)
                                                        {
                                                            paxFare.TotalFare = paxFare.TotalTax + paxFare.BasicFare;
                                                        }
                                                        fare.PaxFares = new List<GDSResModel.PaxFare>();
                                                        fare.PaxFares.Add(paxFare);
                                                        break;
                                                }
                                            }
                                        }
                                        #endregion airPricingSolution
                                        if (airPricingSolution.Name.Equals("air:AirSegmentList", StringComparison.OrdinalIgnoreCase))
                                        {
                                            airSegmentList.LoadXml((airPricingSolution).OuterXml);
                                        }
                                        if (listOfBound.Count > 0)
                                        {
                                            seg = new GDSResModel.Segment();
                                            seg.Bonds = new List<GDSResModel.Bond>();
                                            seg.Fare = new GDSResModel.Fare();
                                            seg.Fare = fare;
                                            seg.SegIndex = contractId.ToString();
                                            seg.Segmentid = segmentid;
                                            seg._flightnumber = flightnumber;
                                            seg.JourneyIndex = journeyIndex;
                                            seg.NearByAirport = "";// IsNearByAirport;
                                            seg.FareIndicator = "";// FareIndicator;
                                            if (fareRoues == "")
                                            {
                                                seg.FareRule = "RefundableWithPenalty";
                                            }
                                            else
                                            {
                                                seg.FareRule = fareRoues;
                                            }

                                            //  seg.BondType = "OutBound";
                                            seg.EngineID = "";// AirService.Engine.TravelPort;
                                            seg.ItineraryKey = TPtransactionId;
                                            seg.SearchId = "";// EngineID;
                                            seg.IsSpecial = "";// IsSpecial;
                                            seg.JourneyType = "";// JourneyType;
                                            if (contractType_.Equals("RoundTrip"))
                                            {
                                                seg.IsRoundTrip = true;
                                                seg.BondType = "OutBound";
                                            }
                                            else
                                            {
                                                seg.BondType = contractType_;
                                            }
                                            foreach (GDSResModel.Bond bond_ in listOfBound)
                                            {
                                                seg.Bonds.Add(bond_);
                                            }
                                            if (seg.Fare.PaxFares != null && seg.Fare.PaxFares.Count > 0)
                                            {
                                                int tmfrom = 4;
                                                int tmto = 0;
                                                // cancelTime = CancellationCharges.GetCancellationTime();
                                                //foreach (AirLine ar in cancelTime.AirLines)
                                                //{
                                                //    if (seg.Bonds[0].Legs[0].AirlineName == ar.Code)
                                                //    {
                                                //        foreach (AirlineClass ac in ar.AirlineClasses)
                                                //        {
                                                //            if (seg.Bonds[0].Legs[0].FareClassOfService == ac.Type)
                                                //            {
                                                //                //airCodeMatch = true;
                                                //                tmfrom = Convert.ToInt32(ac.TimeFrom);
                                                //                tmto = Convert.ToInt32(ac.TimeTo);
                                                //            }
                                                //        }
                                                //    }
                                                //}
                                                //CheckFareRule(seg); // to do

                                                if (paxFare.CancelPenalty == 0)
                                                {
                                                    paxFare.Refundable = false;
                                                    seg.FareRule = "CAN-BEF " + tmfrom + "_" + tmto + ":" + seg.Fare.PaxFares[0].CancelPenalty + "|" + "CHG-BEF " + tmfrom + "_" + tmto + ":" + seg.Fare.PaxFares[0].ChangePenalty + "|" + "EMTFee-" + 250 + "|" + "Msg:As per as airline rules.";
                                                }

                                                else
                                                {
                                                    seg.FareRule = "CAN-BEF " + tmfrom + "_" + tmto + ":" + seg.Fare.PaxFares[0].CancelPenalty + "|" + "CHG-BEF " + tmfrom + "_" + tmto + ":" + seg.Fare.PaxFares[0].ChangePenalty + "|" + "EMTFee-" + 250;
                                                }
                                            }
                                            if (IsFlex)
                                            {
                                                if (journeyIndex == 0)
                                                {
                                                    listOfSegment.Add(seg);
                                                }
                                            }
                                            else
                                            {
                                                listOfSegment.Add(seg);
                                            }
                                            bond = new GDSResModel.Bond();
                                            listOfBound = new ArrayList();
                                            bond.Legs = new List<GDSResModel.Leg>();
                                            contractId++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return listOfSegment;
        }

        public GDSResModel.PnrResponseDetails ParsePNRRsp(string PnrResponse, string contractType_, SimpleAvailabilityRequestModel availibiltyRQGDS)
        {
            listOfFareDetails = new ArrayList();
            List<GDSResModel.Segment> listOfSegment = new List<GDSResModel.Segment>();
            //ArrayList listofTPSegment = null;
            ArrayList listOfBound = new ArrayList();
            int contractId = 1;
            string TPtransactionId = string.Empty;
            string outBoundGroup = string.Empty;
            string inBoundGroup = string.Empty;
            int journeyIndex = 0;
            bool IsFlex = true;
            string _journeyTime = string.Empty;
            //List<Bond> listOfBound = new List<Bond>();
            GDSResModel.Segment seg = null;
            bool airSegstatus = false;
            XmlDocument flightDetailsList = new XmlDocument();
            XmlDocument FareInfoList = new XmlDocument();
            XmlDocument BrandList = null;
            string fareRoues = string.Empty;
            GDSResModel.Fare fare = null;
            Dictionary<string, string> baggageDetais = new Dictionary<string, string>();
            Dictionary<string, string> fareRuleInfo = new Dictionary<string, string>();
            ArrayList listofTPSegment = null;
            TPAirSegment AirSegment = null;

            GDSResModel.Leg leg = null;
            ArrayList paxlist = null;
            PnrResponseDetails pnrResDetail = null;
            pnrResDetail = new PnrResponseDetails();
            pnrResDetail.PaxFareList = new ArrayList();
            pnrResDetail.PaxeDetailList = new ArrayList();
            paxlist = new ArrayList();
            listOfBound = new ArrayList();
            listofTPSegment = new ArrayList();
            //listOfFareDetails
            TravellerDetail travellerDetail = null;
            GDSResModel.PaxFare paxFare = new GDSResModel.PaxFare();
            GDSResModel.FareDetail fareDetails = new GDSResModel.FareDetail();
            bool IsDomestic = true;
            string segmentid = string.Empty;
            string flightnumber = string.Empty;
            XmlDocument pnrDoc = new XmlDocument();
            XmlDocument airSegmentList = new XmlDocument();
            pnrDoc.LoadXml(PnrResponse);
            //doc.LoadXml(lowFareResponse.Replace("C11", "CNN"));
            GDSResModel.Bond bond = new GDSResModel.Bond();
            bond.Legs = new List<GDSResModel.Leg>();
            //cancelTime = new FareCancellationTime();
            foreach (XmlNode rootNode in pnrDoc)
            {
                if (rootNode.Name.Equals("SOAP:Envelope", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (XmlNode root in rootNode.ChildNodes)
                    {
                        if (root.Name.Equals("SOAP:Body", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (XmlNode airCreateReservationRsp in root)
                            {
                                if (airCreateReservationRsp.Name.Equals("universal:UniversalRecordRetrieveRsp", StringComparison.OrdinalIgnoreCase))
                                {
                                    foreach (XmlNode airCreateRes in airCreateReservationRsp)
                                    {
                                        switch (airCreateRes.Name)
                                        {
                                            case "common_v52_0:ResponseMessage":
                                                pnrResDetail.PnrMessage = airCreateRes.InnerText;
                                                if (airCreateRes.Attributes["Code"].Value.Equals("1", StringComparison.OrdinalIgnoreCase) && airCreateRes.InnerText.Equals("TO ENSURE FARE GUARANTEE - TICKET NOW", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    pnrResDetail.PnrStatus = true;
                                                }
                                                else
                                                {
                                                    pnrResDetail.PnrStatus = false;
                                                }
                                                break;
                                            case "universal:UniversalRecord":
                                                pnrResDetail.UniversalRecordLocator = airCreateRes.Attributes["LocatorCode"].Value;
                                                pnrResDetail.DealCodeVersion = airCreateRes.Attributes["Version"].Value;
                                                foreach (XmlNode universalRecord in airCreateRes)
                                                {
                                                    switch (universalRecord.Name)
                                                    {
                                                        case "common_v52_0:BookingTraveler":
                                                            travellerDetail = new TravellerDetail();
                                                            travellerDetail.PaxId = universalRecord.Attributes["Key"].Value;
                                                            switch (universalRecord.Attributes["TravelerType"].Value)
                                                            {
                                                                case "ADT":
                                                                    travellerDetail.PaxType = PAXTYPE.ADT;
                                                                    break;
                                                                case "CNN":
                                                                    travellerDetail.PaxType = PAXTYPE.CHD;
                                                                    break;
                                                                case "C11":
                                                                    travellerDetail.PaxType = PAXTYPE.CHD;
                                                                    break;
                                                                case "CHD":
                                                                    travellerDetail.PaxType = PAXTYPE.CHD;
                                                                    break;
                                                                case "INF":
                                                                    travellerDetail.PaxType = PAXTYPE.INF;
                                                                    break;
                                                            }
                                                            foreach (XmlNode paxName in universalRecord)
                                                            {
                                                                if (paxName.Name.Equals("common_v52_0:BookingTravelerName", StringComparison.OrdinalIgnoreCase))
                                                                {
                                                                    travellerDetail.FirstName = paxName.Attributes["First"].Value;
                                                                    travellerDetail.LastName = paxName.Attributes["Last"].Value;
                                                                }
                                                            }
                                                            paxlist.Add(travellerDetail);
                                                            pnrResDetail.PaxeDetailList.Add(travellerDetail);
                                                            break;
                                                        case "universal:ProviderReservationInfo":
                                                            pnrResDetail.ProviderReservationLocator = universalRecord.Attributes["LocatorCode"].Value;
                                                            pnrResDetail.bookingdate = Convert.ToDateTime(universalRecord.Attributes["CreateDate"].Value);
                                                            break;


                                                        case "air:AirReservation":
                                                            pnrResDetail.AirReservationLocatorCode = universalRecord.Attributes["LocatorCode"].Value;
                                                            foreach (XmlNode airReservation in universalRecord)
                                                            {
                                                                switch (airReservation.Name)
                                                                {
                                                                    case "common_v52_0:SupplierLocator":
                                                                        pnrResDetail.SupplierLocatorCode = airReservation.Attributes["SupplierLocatorCode"].Value;
                                                                        break;
                                                                    case "air:AirSegment":
                                                                        leg = new GDSResModel.Leg();
                                                                        AirSegment = new TPAirSegment();
                                                                        AirSegment.AirSegment = airReservation.Attributes["Key"].Value;
                                                                        AirSegment.AirSegmentDetail = airReservation.OuterXml.Trim();
                                                                        listofTPSegment.Add(AirSegment);
                                                                        if (airReservation.Attributes["Group"].Value.Equals("0", StringComparison.OrdinalIgnoreCase))
                                                                        {
                                                                            leg.BoundType = "OutBound";
                                                                            leg.Group = "0";
                                                                            //bond.BoundType = "OutBound";
                                                                        }
                                                                        else if (airReservation.Attributes["Group"].Value.Equals("1", StringComparison.OrdinalIgnoreCase))
                                                                        {
                                                                            leg.BoundType = "InBound";
                                                                            leg.Group = "1";
                                                                            //bond.BoundType = "InBound";
                                                                        }
                                                                        if (airReservation.Attributes["NumberOfStops"] != null)
                                                                        {
                                                                            leg.NumberOfStops = airReservation.Attributes["NumberOfStops"].Value;
                                                                        }
                                                                        else
                                                                        {
                                                                            leg.NumberOfStops = "0";
                                                                        }
                                                                        leg.FlightNumber = airReservation.Attributes["FlightNumber"].Value;
                                                                        leg.AirlineName = airReservation.Attributes["Carrier"].Value;
                                                                        leg.CarrierCode = airReservation.Attributes["Carrier"].Value;
                                                                        leg.Origin = airReservation.Attributes["Origin"].Value;
                                                                        leg.Destination = airReservation.Attributes["Destination"].Value;
                                                                        //leg.DepartureDate = Utility.Utility.GDateFormate(airSegment.Attributes["DepartureTime"].Value.Split('T')[0], Utility.Engine.TravelPort);
                                                                        leg.DepartureTime = airReservation.Attributes["DepartureTime"].Value;// airSegment.Attributes["DepartureTime"].Value.Split('T')[1];
                                                                                                                                             //leg.ArrivalDate = Utility.Utility.GDateFormate(airSegment.Attributes["ArrivalTime"].Value.Split('T')[0], Utility.Engine.TravelPort);
                                                                        leg.ArrivalTime = airReservation.Attributes["ArrivalTime"].Value;// airSegment.Attributes["ArrivalTime"].Value.Split('T')[1];
                                                                        leg.FareClassOfService = airReservation.Attributes["ClassOfService"].Value;
                                                                        if (airReservation.Attributes["FlightTime"] != null)
                                                                        {
                                                                            leg.Duration = airReservation.Attributes["FlightTime"].Value;
                                                                        }
                                                                        else
                                                                        {
                                                                            leg.Duration = "120";
                                                                        }
                                                                        leg.AircraftCode = airReservation.Attributes["Key"].InnerText;
                                                                        leg.Group = airReservation.Attributes["Group"].Value;
                                                                        foreach (XmlNode airSegmentChild in airReservation)
                                                                        {
                                                                            switch (airSegmentChild.Name)
                                                                            {
                                                                                case "air:AirAvailInfo":
                                                                                    leg.ProviderCode = airSegmentChild.Attributes["ProviderCode"].Value;
                                                                                    break;
                                                                                case "air:FlightDetails":
                                                                                    leg.FlightDetailRefKey = airSegmentChild.Attributes["Key"].Value;
                                                                                    if (airSegmentChild.Attributes["OriginTerminal"] != null)
                                                                                    {
                                                                                        leg.DepartureTerminal = airSegmentChild.Attributes["OriginTerminal"].Value;
                                                                                    }
                                                                                    if (airSegmentChild.Attributes["DestinationTerminal"] != null)
                                                                                    {
                                                                                        leg.ArrivalTerminal = airSegmentChild.Attributes["DestinationTerminal"].Value;
                                                                                    }
                                                                                    break;
                                                                            }
                                                                        }
                                                                        switch (leg.AirlineName)
                                                                        {
                                                                            case "AI":
                                                                                leg.FlightName = "AirIndia";
                                                                                break;
                                                                            case "9W":
                                                                                leg.FlightName = "JetAirWays";
                                                                                break;
                                                                            case "UK":
                                                                                leg.FlightName = "Vistara";
                                                                                break;
                                                                            case "OD":
                                                                                leg.FlightName = "MALINDO AIRWAYS";
                                                                                break;
                                                                            case "TG":
                                                                                leg.FlightName = "Thai Airways International";
                                                                                break;
                                                                            case "UL":
                                                                                leg.FlightName = "SriLankan Airlines";
                                                                                break;
                                                                            default:
                                                                                try
                                                                                {
                                                                                    //leg.FlightName = Utility.FileChangeMonitor.AirlineNames[leg.AirlineName.Trim()];
                                                                                }
                                                                                catch (SystemException sex_) { leg.FlightName = leg.AirlineName.Trim(); }
                                                                                break;
                                                                        }
                                                                        bond.Legs.Add(leg);
                                                                        //pnrResDetail.Bonds = bond;
                                                                        break;
                                                                    case "air:AirPricingInfo":
                                                                        pnrResDetail.IsAirPricingInfo = true;
                                                                        paxFare = new GDSResModel.PaxFare();
                                                                        paxFare.FareID = airReservation.Attributes["Key"].Value;
                                                                        paxFare.BasicFare = Convert.ToDecimal(airReservation.Attributes["ApproximateBasePrice"].Value.Remove(0, 3));
                                                                        paxFare.TotalTax = Convert.ToDecimal(airReservation.Attributes["Taxes"].Value.Remove(0, 3));
                                                                        if (airReservation.Attributes["PlatingCarrier"] != null)
                                                                        {
                                                                            pnrResDetail.PlatingCarrier = airReservation.Attributes["PlatingCarrier"].Value;
                                                                        }
                                                                        foreach (XmlNode paxType in airReservation)
                                                                        {
                                                                            if (paxType.Name.Equals("air:PassengerType", StringComparison.OrdinalIgnoreCase))
                                                                            {
                                                                                switch (paxType.Attributes["Code"].Value)
                                                                                {
                                                                                    case "ADT":
                                                                                        paxFare.PaxType = PAXTYPE.ADT;
                                                                                        break;
                                                                                    case "CNN":
                                                                                        paxFare.PaxType = PAXTYPE.CHD;
                                                                                        break;
                                                                                    case "C11":
                                                                                        paxFare.PaxType = PAXTYPE.CHD;
                                                                                        break;
                                                                                    case "CHD":
                                                                                        paxFare.PaxType = PAXTYPE.CHD;
                                                                                        break;
                                                                                    case "INF":
                                                                                        paxFare.PaxType = PAXTYPE.INF;
                                                                                        break;
                                                                                }
                                                                            }
                                                                        }
                                                                        listOfFareDetails.Add(paxFare);
                                                                        pnrResDetail.PaxFareList.Add(paxFare);
                                                                        break;
                                                                    case "":
                                                                        break;
                                                                }
                                                            }
                                                            break;
                                                        case "common_v52_0:FormOfPayment":
                                                            if (universalRecord.Attributes["Type"].Value.Equals("Credit", StringComparison.OrdinalIgnoreCase))
                                                                pnrResDetail.IsCreditPayment = true;
                                                            pnrResDetail.PaymentKey = universalRecord.Attributes["Key"].Value;
                                                            break;
                                                        case "":
                                                            break;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (bond.Legs.Count > 0)
            {
                pnrResDetail.Bonds = new Bond();
                pnrResDetail.Bonds = bond;
            }

            return pnrResDetail;
        }

    }
}
