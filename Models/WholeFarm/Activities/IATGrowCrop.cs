﻿using Models.Core;
using Models.WholeFarm.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Models.WholeFarm.Activities
{
	/// <summary>Grow a crop activity</summary>
	/// <summary>This activity sows, grows and harvests crops.</summary>
	/// <version>1.0</version>
	/// <updates>First implementation of this activity recreating IAT logic</updates>
	[Serializable]
	[ViewName("UserInterface.Views.GridView")]
	[PresenterName("UserInterface.Presenters.PropertyPresenter")]
	[ValidParent(ParentType = typeof(IATCropLand))]
	public class IATGrowCrop: WFActivityBase
	{
        [Link]
		Clock Clock = null;

        //[Link]
        //ISummary Summary = null;

        [Link]
        Simulation Simulation = null;


        [Link]
        private ResourcesHolder Resources = null;



        /// <summary>
        /// Name of the model for the crop input file
        /// </summary>
        [Description("Name of model for crop growth file")]
        public string ModelNameFileCrop { get; set; }

        /// <summary>
        /// Name of crop in file
        /// </summary>
        [Description("Name of crop in file")]
		public string CropName { get; set; }





        /// <summary>
        /// Store to put crop growth into
        /// </summary>
        [Description("Store to put crop growth into")]
        public StoresForCrops Store { get; set; }


        /// <summary>
        /// Item name (in the store) to put crop growth into
        /// </summary>
        [Description("Item name (in the store) to put crop growth into")]
        public string StoreItemName { get; set; }

        /// <summary>
        /// Percentage of the crop growth that is kept
        /// </summary>
        [Description("Proportion of crop growth kept (%)")]
        public double PercentKept { get; set; }







        /// <summary>
        /// Human Food item
        /// </summary>
        [XmlIgnore]
        public HumanFoodStoreType LinkedHumanFoodItem { get; set; }

        /// <summary>
        /// Feed item
        /// </summary>
        [XmlIgnore]
		public AnimalFoodStoreType LinkedAnimalFoodItem { get; set; }

        /// <summary>
        /// Inedible crop product item
        /// </summary>
        [XmlIgnore]
        public ProductStoreType LinkedProductItem { get; set; }

        /// <summary>
        /// Harvest Data retrieved from the Forage File.
        /// </summary>
        [XmlIgnore]
        public List<CropDataType> HarvestData { get; set; }


        /// <summary>
        /// Model for the crop input file
        /// </summary>
        private FileCrop fileCrop;

        /// <summary>
        /// Parent of this Model that gets the land for growing this crop.
        /// </summary>
        private IATCropLand cropLand;




        /// <summary>An event handler to allow us to initialise ourselves.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
		private void OnSimulationCommencing(object sender, EventArgs e)
		{
            fileCrop = Apsim.Child(Simulation, ModelNameFileCrop) as FileCrop;
            if (fileCrop == null)
            {
                throw new ApsimXException(this, String.Format("Unable to locate model for crop input file {0} (under Simulation) referred to in {1}", this.ModelNameFileCrop, this.Name));
            }


            switch (Store)
            {
                case StoresForCrops.HumanFoodStore:
                    LinkedHumanFoodItem = Resources.GetResourceItem(this, typeof(HumanFoodStore), StoreItemName, OnMissingResourceActionTypes.ReportErrorAndStop, OnMissingResourceActionTypes.ReportErrorAndStop) as HumanFoodStoreType;
                    LinkedAnimalFoodItem = Resources.GetResourceItem(this, typeof(AnimalFoodStore), StoreItemName, OnMissingResourceActionTypes.ReportErrorAndStop, OnMissingResourceActionTypes.ReportErrorAndStop) as AnimalFoodStoreType;
                    break;
                case StoresForCrops.AnimalFoodStore:
                    LinkedAnimalFoodItem = Resources.GetResourceItem(this, typeof(AnimalFoodStore), StoreItemName, OnMissingResourceActionTypes.ReportErrorAndStop, OnMissingResourceActionTypes.ReportErrorAndStop) as AnimalFoodStoreType;
                    break;
                case StoresForCrops.ProductStore:
                    LinkedProductItem = Resources.GetResourceItem(this, typeof(ProductStore), StoreItemName, OnMissingResourceActionTypes.ReportErrorAndStop, OnMissingResourceActionTypes.ReportErrorAndStop) as ProductStoreType;
                    break;
                default:
                    throw new Exception(String.Format("Store {0} is not supported for {1}", Enum.GetName(typeof(StoresForCrops), Store), this.Name));
            }


            cropLand = (IATCropLand)this.Parent;

            // Retrieve harvest data from the forage file for the entire run. 
            HarvestData = fileCrop.GetCropDataForEntireRun(cropLand.LinkedLandItem.SoilType, CropName, 
                                                               Clock.StartDate, Clock.EndDate);
            if (HarvestData == null)
            {
                throw new ApsimXException(this, String.Format("Unable to locate in crop file {0} any harvest data for SoilType {1}, CropName {2} between the dates {3} and {4}", 
                    fileCrop.FileName, cropLand.LinkedLandItem.SoilType, CropName, Clock.StartDate, Clock.EndDate));
            }
            
            
        }



        /// <summary>An event handler to allow us to initialise</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("WFInitialiseActivity")]
        private void OnWFInitialiseActivity(object sender, EventArgs e)
        {

        }




        /// <summary>
        /// Method to determine resources required for this activity in the current month
        /// </summary>
        /// <returns>A list of resource requests</returns>
        public override List<ResourceRequest> GetResourcesNeededForActivity()
        {
            return null;
        }



        /// <summary>
        /// Method used to perform activity if it can occur as soon as resources are available.
        /// </summary>
        public override void DoActivity()
		{
			return;
		}

		/// <summary>
		/// Method to determine resources required for initialisation of this activity
		/// </summary>
		/// <returns></returns>
		public override List<ResourceRequest> GetResourcesNeededForinitialisation()
		{
			return null;
		}

		/// <summary>
		/// Method used to perform initialisation of this activity.
		/// This will honour ReportErrorAndStop action but will otherwise be preformed regardless of resources available
		/// It is the responsibility of this activity to determine resources provided.
		/// </summary>
		public override void DoInitialisation()
		{
			return;
		}

		/// <summary>An event handler for a Cut and Carry</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		[EventSubscribe("WFDoCutAndCarry")]
        private void OnWFDoCutAndCarry(object sender, EventArgs e)
        {
            int year = Clock.Today.Year;
            int month = Clock.Today.Month;

            //nb. we are relying on the fact that the HarvestData list is sorted by date.
            CropDataType nextHarvest = HarvestData.FirstOrDefault();

            if (nextHarvest != null)
            {
                //if this month is a harvest month for this crop
                if ((year == nextHarvest.Year) && (month == nextHarvest.Month))
                {
                    double totalamount = nextHarvest.AmtKg * cropLand.Area * (PercentKept / 100);

                    switch (Store)
                    {
                        case StoresForCrops.HumanFoodStore:
                            if (totalamount > 0)
                            {
                                //TODO: check that there is no N provided with grain
                                LinkedHumanFoodItem.Add(totalamount, this.Name, "Harvest");
                            }
                            break;
            
                        case StoresForCrops.AnimalFoodStore:
                            if (totalamount > 0)
                            {
                                //if Npct column was not in the file 
                                if (nextHarvest.Npct == double.NaN)
                                {
                                    //Add without adding any new nitrogen.
                                    //The nitrogen value for this feed item in the store remains the same.
                                    LinkedAnimalFoodItem.Add(totalamount, this.Name, "Harvest");
                                }
                                else
                                {
                                    FoodResourcePacket packet = new FoodResourcePacket()
                                    {
                                        Amount = totalamount,
                                        PercentN = nextHarvest.Npct
                                    };
                                    LinkedAnimalFoodItem.Add(packet, this.Name, "Harvest");
                                }
                            }
                            break;

                        case StoresForCrops.ProductStore:
                            if (totalamount > 0)
                            {
                                LinkedProductItem.Add(totalamount, this.Name, "Harvest");
                            }
                            break;

                        default:
                            throw new Exception(String.Format("Store {0} is not supported for {1}", Enum.GetName(typeof(StoresForCrops), Store), this.Name));
                    }



                    //Now remove the first item from the harvest data list because it has happened.

                    //This causes a problem for the children of this model 
                    //because of a race condition that occurs if user sets MthsBeforeHarvest = 0 
                    //for any of the children of this model. 
                    //Then because this model and children are all executing on the harvest month and 
                    //this executes first and it removes the harvest from the harvest list, 
                    //then the chidren never get the Clock.Today == harvest date (aka costdate).
                    //So to fix this problem, in the children we store the next harvest date (aka costdate) 
                    //in a global variable and don't update its value
                    //until after we have done the Clock.Today == harvest date (aka costdate) comparison.

                    HarvestData.RemoveAt(0);  
                }
            }
        }



        /// <summary>
        /// Resource shortfall event handler
        /// </summary>
        public override event EventHandler ResourceShortfallOccurred;

		/// <summary>
		/// Shortfall occurred 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnShortfallOccurred(EventArgs e)
		{
			if (ResourceShortfallOccurred != null)
				ResourceShortfallOccurred(this, e);
		}
	}

}
