// See https://aka.ms/new-console-template for more information

using Simple.OData.Client;
using Microsoft.OData.Edm;
//using Microsoft.OData.Edm.Csdl;
//using Microsoft.OData.Edm.Csdl.CsdlSemantics;
using Microsoft.Data.Edm;


using OData2Poco.Api;


class Program
{

    private static string _svcurl = "https://services.odata.org/TripPinRESTierService/(S(jcidpq3drpzq0hefuabvtj5f))/"; // v4
//    private static string _svcurl = "https://services.odata.org/V2/OData/OData.svc/"; // v2
    static async Task<int> Main(string[] args)
    {

        var cl = new ODataClient( _svcurl );


        var modelabstr = await cl.GetMetadataAsync();
        string modeltxt = await cl.GetMetadataAsStringAsync();

        if ( modelabstr.GetType().FullName.StartsWith("Microsoft.Data.Edm") )
        {
            Microsoft.Data.Edm.IEdmModel model =  modelabstr as Microsoft.Data.Edm.IEdmModel ;
            
            var entitySets = model.EntityContainers().FirstOrDefault().EntitySets();
            foreach ( var entitySet in entitySets )
            {
                Console.WriteLine( entitySet.Name + "/" ); //+ entitySet );
            }

        }
        else
        {
            var model = (Microsoft.OData.Edm.EdmModelBase) await cl.GetMetadataAsync();
        
            var entitySets = model.EntityContainer.EntitySets();

            foreach ( var entitySet in entitySets )
            {
                Console.WriteLine( entitySet.Name + "/" + entitySet.EntityType().Name );

    
                var type = entitySet.EntityType();
                foreach ( var structProp in type.DeclaredStructuralProperties() )
                {
                    if ( !structProp.Type.IsCollection() )
                    {
                        var def = (Microsoft.OData.Edm.IEdmNamedElement) structProp.Type.Definition;
                        Console.WriteLine("---Prop: " + structProp.Name + " / " + def.Name );
                    }
                    else
                    {
                        var def = (Microsoft.OData.Edm.IEdmCollectionType) structProp.Type.Definition;
                        var elemType = (Microsoft.OData.Edm.IEdmNamedElement) def.ElementType.Definition; 
                        Console.WriteLine("---Prop: " + structProp.Name + " / " + $"Collection({elemType.Name})" );
                    }
                }

                foreach ( var navProp in type.DeclaredNavigationProperties() )
                {
                    Console.WriteLine("---NavProp: " + navProp.Name + " / " + navProp.Type.GetType().Name );
                }
            }
    


        }
//        Console.WriteLine("Hello, World!");
        string tmpfile = Guid.NewGuid() + ".xml";
        await File.WriteAllTextAsync( tmpfile, modeltxt );

        var o2pconnstring = new OData2Poco.OdataConnectionString 
        {
            ServiceUrl = tmpfile
        };

        var o2psetting = new OData2Poco.PocoSetting
        {
            AddNavigation = true

            
//            AddKeyAttribute = true, 
            
        };

        var o2p = new O2P( o2psetting );
        var code = await o2p.GenerateAsync( o2pconnstring );

        File.Delete( tmpfile );
        

        Console.WriteLine( code );

        return 0;

    }

}