using System;
using System.Collections.Generic;
using System.Linq;
using Lusid.Sdk.Api;
using Lusid.Sdk.Client;
using Lusid.Sdk.Model;
using Lusid.Sdk.Examples.Utilities;
using Lusid.Sdk.Utilities;
using LusidFeatures;
using NUnit.Framework;

namespace Lusid.Sdk.Examples.Tutorials.MarketData
{
    [TestFixture]
    public class Instruments: TutorialBase
    {
        
        private const string FigiScheme = "Figi";
        private const string CustomInternalScheme = "ClientInternal";
        private const string IsinScheme = "Isin";
        private const string SedolScheme = "Sedol";
        
        private const string IsinPropertyKey = "Instrument/default/Isin";
        private const string SedolPropertyKey = "Instrument/default/Sedol";
        private const string FigiPropertyKey = "Instrument/default/Figi";
            
        [OneTimeSetUp]
        public void SetUp()
        {
            EnsurePropertyDefinition("CustomSector");
            SeedInstrumentMaster();
        }

        private void SeedInstrumentMaster()
        {
            var upsertInstrumentsResponse = InstrumentsApi.UpsertInstruments(new Dictionary<string, InstrumentDefinition>
            {
                ["correlationId1"] = new InstrumentDefinition(
                    name: "VODAFONE GROUP PLC", 
                    
                    /*
                        Instruments are created with a set of identifiers
                        each under a different scheme
                     */
                    identifiers: new Dictionary<string, InstrumentIdValue>
                    {
                        [FigiScheme] = new InstrumentIdValue("BBG000C6K6G9"),
                        [CustomInternalScheme] = new InstrumentIdValue("INTERNAL_ID_1"),
                        [IsinScheme] = new InstrumentIdValue("GB00BH4HKS39"),
                        [SedolScheme] = new InstrumentIdValue("BH4HKS3")
                    }                    
                 ),
                
                ["correlationId2"] = new InstrumentDefinition(
                    name: "BARCLAYS PLC", 
                    
                    /*
                        Instruments are created with a set of identifiers
                        each under a different scheme
                     */
                    identifiers: new Dictionary<string, InstrumentIdValue>
                    {
                        [FigiScheme] = new InstrumentIdValue("BBG000C04D57"),
                        [CustomInternalScheme] = new InstrumentIdValue("INTERNAL_ID_2"),
                        [IsinScheme] = new InstrumentIdValue("GB0031348658"),
                        [SedolScheme] = new InstrumentIdValue("3134865")
                    }
                ),
                
                ["correlationId3"] = new InstrumentDefinition(
                    name: "NATIONAL GRID PLC", 
                    
                    /*
                        Instruments are created with a set of identifiers
                        each under a different scheme
                     */
                    identifiers: new Dictionary<string, InstrumentIdValue>
                    {
                        [FigiScheme] = new InstrumentIdValue("BBG000FV67Q4"),
                        [CustomInternalScheme] = new InstrumentIdValue("INTERNAL_ID_3"),
                        [IsinScheme] = new InstrumentIdValue("GB00BDR05C01"),
                        [SedolScheme] = new InstrumentIdValue("BDR05C0")
                    }
                ),
                
                ["correlationId4"] = new InstrumentDefinition(
                    name: "SAINSBURY (J) PLC", 
                    
                    /*
                        Instruments are created with a set of identifiers
                        each under a different scheme
                     */
                    identifiers: new Dictionary<string, InstrumentIdValue>
                    {
                        [FigiScheme] = new InstrumentIdValue("BBG000BF0KW3"),
                        [CustomInternalScheme] = new InstrumentIdValue("INTERNAL_ID_4"),
                        [IsinScheme] = new InstrumentIdValue("GB00B019KW72"),
                        [SedolScheme] = new InstrumentIdValue("B019KW7")
                    }
                ),
                
                ["correlationId5"] = new InstrumentDefinition(
                    name: "TAYLOR WIMPEY PLC", 
                    
                    /*
                        Instruments are created with a set of identifiers
                        each under a different scheme
                     */
                    identifiers: new Dictionary<string, InstrumentIdValue>
                    {
                        [FigiScheme] = new InstrumentIdValue("BBG000BF4KL1"),
                        [CustomInternalScheme] = new InstrumentIdValue("INTERNAL_ID_5"),
                        [IsinScheme] = new InstrumentIdValue("GB0008782301"),
                        [SedolScheme] = new InstrumentIdValue("0878230")
                    }
                )
            });
            
            Assert.That(upsertInstrumentsResponse.Values, Has.Count.EqualTo(5));
        }
        
        [LusidFeature("F22")]
        [Test]
        public void Lookup_Instrument_By_Unique_Id()
        {
            /*
                Look up an instrument that already exists in the instrument master by a
                unique id, in this case an OpenFigi, and also return a list of aliases
            */

            var lookedUpInstruments = InstrumentsApi.GetInstruments(
                FigiScheme, 
                new List<string> {"BBG000C6K6G9"},
                propertyKeys: new List<string> {IsinPropertyKey, SedolPropertyKey});
            
            Assert.That(lookedUpInstruments.Values, Contains.Key("BBG000C6K6G9"));

            var instrument = lookedUpInstruments.Values["BBG000C6K6G9"];
            
            Assert.That(instrument.Name, Is.EqualTo("VODAFONE GROUP PLC"));

            var identifiers = instrument.Properties.OrderBy(i => i.Key).ToList();
            
            Assert.That(identifiers[0].Key, Is.EqualTo(IsinPropertyKey));
            Assert.That(identifiers[0].Value.LabelValue, Is.EqualTo("GB00BH4HKS39"));
            Assert.That(identifiers[1].Key, Is.EqualTo(SedolPropertyKey));
            Assert.That(identifiers[1].Value.LabelValue, Is.EqualTo("BH4HKS3"));
        }
        
         private void EnsurePropertyDefinition(string code)
        {
            var propertyApi = ApiFactory.Api<IPropertyDefinitionsApi>();
                
            try
            {
                propertyApi.GetPropertyDefinition("Instrument", TestDataUtilities.TutorialScope, code);
            }
            catch (ApiException)
            {
                //    Property definition doesn't exist (returns 404), so create one
                //    Details of the property to be created
                var propertyDefinition = new CreatePropertyDefinitionRequest(
                    domain: CreatePropertyDefinitionRequest.DomainEnum.Instrument,
                    scope: TestDataUtilities.TutorialScope,
                    lifeTime: CreatePropertyDefinitionRequest.LifeTimeEnum.Perpetual,
                    code: code,
                    valueRequired: false,
                    displayName: code,
                    dataTypeId: new ResourceId("system", "string")
                );

                //    Create the property
                propertyApi.CreatePropertyDefinition(propertyDefinition);
            }
        }
        
        [LusidFeature("F23")]
        [Test]
        public void List_Available_Identifiers()
        {
            //    Get the list of identifier schemes
            ResourceListOfInstrumentIdTypeDescriptor identifiers = InstrumentsApi.GetInstrumentIdentifierTypes();
            Assert.Greater(identifiers.Values.Count, 0);
        }
        
        [LusidFeature("F24")]
        [Test]
        public void List_All_Instruments()
        {
            const int pageSize = 5;
            
            //    List the instruments restricting, the number that are returned
            var instruments = InstrumentsApi.ListInstruments(limit: pageSize);
            
            Assert.That(instruments.Values.Count(), Is.LessThanOrEqualTo(pageSize));
        }
        
        [LusidFeature("F25")]
        [Test]
        public void List_Instruments_By_Identifier_Type()
        {
            var figis = new List<string>
            {
                "BBG000BF4KL1", "BBG000BF0KW3", "BBG000FV67Q4"
            };
            
            //    Get a set of instruments querying by FIGIs
            var instruments = InstrumentsApi.GetInstruments(identifierType: "Figi", requestBody: figis);

            foreach (var figi in figis)
            {
                Assert.That(instruments.Values, Contains.Key(figi));
            }
        }
        
        [LusidFeature("F26")]
        [Test]
        public void Edit_Instrument_Property()
        {
            const string identifierType = "Figi";
            const string figi = "BBG000BF4KL1";
            
            //    Create the property value
            var propertyValue = new PropertyValue(labelValue: "Construction");
            var propertyKey = $"Instrument/{TestDataUtilities.TutorialScope}/CustomSector";

            //    Add it to the instrument
            InstrumentsApi.UpsertInstrumentsProperties(new List<UpsertInstrumentPropertyRequest>
            {
                new UpsertInstrumentPropertyRequest(
                    identifierType: identifierType,
                    identifier: figi,
                    properties: new List<Property>
                    {
                        new Property(propertyKey, propertyValue)
                    }
                )
            });
            
            //    Get the instrument with value
            Instrument instrument = InstrumentsApi.GetInstrument(
                identifierType: identifierType,
                identifier: figi,
                propertyKeys: new List<string> { propertyKey }
            );

            Assert.That(
                instrument.Properties.FirstOrDefault(p => p.Key == propertyKey && p.Value.LabelValue.Equals("Construction")),
                Is.Not.Null, $"Cannot find property {propertyKey} with value {propertyValue.LabelValue}");
        }
        
        [LusidFeature("F27")]
        [Test]
        public void Create_Custom_Instrument()
        {
            //    Create a definition for the instrument
            var swapDefinition = new InstrumentDefinition(
                name: "10mm 5Y Fixed",
                
                //  The set of identifiers used for identifying the instrument
                //  e.g. for uploading transactions
                identifiers: new Dictionary<string, InstrumentIdValue>
                {
                    ["ClientInternal"] = new InstrumentIdValue(value: "SW-1")
                },
                
                //  The details for valuing the instrument
                definition: new ExoticInstrument(
                    
                    //  Identifies which valuation engine to use
                    instrumentFormat: new InstrumentDefinitionFormat("CustomSource", "CustomFormat", "0.1.2"),
                    content: "<customFormat>upload in custom xml or JSON format</customFormat>",
                    instrumentType: LusidInstrument.InstrumentTypeEnum.ExoticInstrument
                ));
            
            //    create the swap
            var createSwapResponse = InstrumentsApi.UpsertInstruments(new Dictionary<string, InstrumentDefinition>
            {
                ["correlationId"] = swapDefinition
            });
            
            Assert.That(createSwapResponse.Failed, Is.Empty);
        }
    }
}
