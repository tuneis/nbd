﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NBDCostTrackingSystem
{
    public partial class ViewAllProductionPlans : System.Web.UI.Page
    {
        public static EntitiesNBD db = new EntitiesNBD();
        protected void Page_Load(object sender, EventArgs e)
        {
            //check is user is logged in
            if (User.Identity.IsAuthenticated)
            {
                // Find the control on the master page and assign the username to the label
                Label userNameLabel = (Label)Page.Master.FindControl("lblUserLoggedIn");
                userNameLabel.Text = User.Identity.Name;
            }
            else
            {
                Response.Redirect("~/Default.aspx");
            }
            if (!IsPostBack)
            {
                //Populate Client Listbox
                var clientList = db.CLIENTs.ToList();
                foreach (var n in clientList)
                {
                    ListItem li = new ListItem();
                    li.Text = n.cliName;
                    li.Value = n.ID.ToString();
                    lbClientResults.Items.Add(li);
                }
            }
        }

        protected void lbClientResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get client id from listbox
            int ID = Convert.ToInt32(lbClientResults.SelectedValue);

            // Find project related to client
            var projectSearch = db.PROJECTs
                               .Where(t =>
                                   t.clientID == ID)
                               .ToList();

            // Check for no results
            if (projectSearch.Count == 0)
            {
                lbProjectResults.Items.Clear();
                ListItem error = new ListItem();
                error.Attributes.Add("disabled", "disabled");
                error.Text = "No Projects Found for that Client!";
                lbProjectResults.Items.Add(error);
            }
            else
            {
                lbProjectResults.Items.Clear();
                foreach (var n in projectSearch)
                {
                    ListItem li = new ListItem();
                    li.Text = n.projName;
                    li.Value = n.ID.ToString();
                    lbProjectResults.Items.Add(li);
                }
            }
        }

        protected void lbProjectResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Turn on panel
            showBid.Visible = true;

            //Create Total Variables
            decimal materialsTotal = 0;
            decimal labourTotal = 0;
            decimal bidTotal = 0;
            decimal labourPercent = 0;

            // Pull the project information from the DB
            int projectID = Convert.ToInt32(lbProjectResults.SelectedValue);
            var modalResults = db.PROJECTs
                .Where(t =>
                t.ID == projectID)
                .ToList();

            foreach (var r in modalResults)
            {
                // Assign results to variables
                string contactFName = r.CLIENT.cliConFName;
                string contactLName = r.CLIENT.cliConLName;
                string clientAddress = r.CLIENT.cliAddress;
                string clientName = r.CLIENT.cliName;
                string clientPhone = r.CLIENT.cliPhone;
                string projectName = r.projName;
                string estimatedStart = r.projEstStart;
                string estimatedEnd = r.projEstEnd;
                string projSite = r.projSite;
                int designerID = r.designerID;

                // Get designer information
                var getDesigner = db.WORKERs.Where(t => t.ID == designerID).ToList();
                foreach (var d in getDesigner)
                {
                    string designerName = d.wrkFName + " " + d.wrkLName;
                    mdlTxtNBDDesigner.Text = designerName;
                }

                // Get sales associate information
                var getSalesAssoc = from p in db.PROJECTs
                                    join pt in db.PROD_TEAM on p.ID equals pt.projectID
                                    join tm in db.TEAM_MEMBER on pt.ID equals tm.teamID
                                    join w in db.WORKERs on tm.workerID equals w.ID
                                    where pt.projectID == projectID
                                    where w.wrkTypeID == 7
                                    select w;
                foreach (var w in getSalesAssoc)
                {
                    string salesAssocName = w.wrkFName + " " + w.wrkLName;
                    mdlTxtNBDSales.Text = salesAssocName;
                }

                // Assign controls data
                mdlTxtClientContact.Text = contactFName + " " + contactLName;
                mdlTxtClientAddress.Text = clientAddress;
                mdlTxtClientName.Text = clientName;
                mdlTxtClientPhone.Text = clientPhone;
                mdlTxtProjectName.Text = projectName;
                mdlTxtProjectSite.Text = projSite;
                mdlTxtProjectBeginDate.Text = estimatedStart;
                mdlTxtProjectEndDate.Text = estimatedEnd;

                //START PULL MATERIAL TABLE
                tblMaterialsSummary.Rows.Clear();
                TableHeaderRow mth = new TableHeaderRow();
                mth.TableSection = TableRowSection.TableHeader;
                string[] materialHeaderArray = { "#", "Type", "Quantity", "Description", "Size", "Unit Price", "Extended Price" };

                for (int i = 0; i < materialHeaderArray.Length; i++)
                {
                    TableCell cell = new TableCell();
                    cell.Font.Bold = true;
                    cell.Text = materialHeaderArray[i];
                    mth.Cells.Add(cell);
                }

                // Add table header to the table
                tblMaterialsSummary.Rows.Add(mth);

                // Material Query
                var materialQuery = from materialReq in db.MATERIAL_REQ
                                    join inventory in db.INVENTORies on materialReq.inventoryID equals inventory.ID
                                    join material in db.MATERIALs on inventory.materialID equals material.ID
                                    where materialReq.projectID == projectID
                                    select materialReq;
                int materialCount = 1;

                foreach (var w in materialQuery)
                {
                    // Create rows and cells
                    TableRow tr = new TableRow();
                    TableCell matCount = new TableCell();
                    TableCell matType = new TableCell();
                    TableCell matQty = new TableCell();
                    TableCell matDesc = new TableCell();
                    TableCell matSize = new TableCell();
                    TableCell matUnPrice = new TableCell();
                    TableCell matExPrice = new TableCell();

                    // Assign data to cells
                    matCount.Text = materialCount.ToString();
                    matType.Text = w.INVENTORY.MATERIAL.matType;
                    matQty.Text = w.mreqEstQty.ToString();
                    matDesc.Text = w.INVENTORY.MATERIAL.matDesc;

                    //Not all the units have a size amount category. If the first is null just set it to 1
                    matSize.Text = (w.INVENTORY.invSizeAmnt == null) ? "1 " + w.INVENTORY.invSizeUnit : w.INVENTORY.invSizeAmnt.ToString() + " " + w.INVENTORY.invSizeUnit;
                    matUnPrice.Text = string.Format("{0:C}", Math.Round(Convert.ToDecimal(w.INVENTORY.invList), 2));
                    matExPrice.Text = string.Format("{0:C}", Math.Round(Convert.ToDecimal(w.INVENTORY.invList * w.mreqEstQty), 2));

                    // Add cells to row
                    tr.Cells.Add(matCount);
                    tr.Cells.Add(matType);
                    tr.Cells.Add(matQty);
                    tr.Cells.Add(matDesc);
                    tr.Cells.Add(matSize);
                    tr.Cells.Add(matUnPrice);
                    tr.Cells.Add(matExPrice);

                    //Increse Row Count
                    materialCount++;

                    //Increase Materials Total
                    materialsTotal += Convert.ToDecimal(w.INVENTORY.invList * w.mreqEstQty);

                    //Add Row to Table
                    tblMaterialsSummary.Rows.Add(tr);
                }
                //END PULL MATERIAL TABLE
                //
                //START PULL LABOUR TABLE
                // Labour Query
                tblLabourSummary.Rows.Clear();
                TableHeaderRow lth = new TableHeaderRow();
                lth.TableSection = TableRowSection.TableHeader;
                string[] labourHeaderArray = { "#", "Type", "Task", "Hours", "Unit Price", "Extended Price" };
                for (int i = 0; i < labourHeaderArray.Length; i++)
                {
                    TableCell cell = new TableCell();
                    cell.Font.Bold = true;
                    cell.Text = labourHeaderArray[i];
                    lth.Cells.Add(cell);
                }

                // Add table header to the table
                tblLabourSummary.Rows.Add(lth);
                var labourQuery = from labourReq in db.LABOUR_REQUIREMENT
                                  join workerType in db.WORKER_TYPE on labourReq.workerTypeID equals workerType.ID
                                  join task in db.TASKs on labourReq.taskID equals task.ID
                                  where labourReq.projectID == projectID
                                  select new { labourReq, workerType, task };
                int labourCount = 1;

                foreach (var w in labourQuery)
                {
                    // Creat cells and rows
                    TableRow tr = new TableRow();
                    TableCell labCount = new TableCell();
                    TableCell labType = new TableCell();
                    TableCell labTask = new TableCell();
                    TableCell labHours = new TableCell();
                    TableCell labUnPrice = new TableCell();
                    TableCell labExPrice = new TableCell();

                    // Assign data to cells
                    labCount.Text = labourCount.ToString();
                    labType.Text = w.workerType.wrkTypeDesc;
                    labTask.Text = w.task.taskDesc;
                    labHours.Text = w.labourReq.lreqEstHours.ToString();
                    labUnPrice.Text = string.Format("{0:C}", Math.Round(Convert.ToDecimal(w.workerType.wrkTypePrice), 2));
                    labExPrice.Text = string.Format("{0:C}", Math.Round(Convert.ToDecimal(w.workerType.wrkTypePrice * w.labourReq.lreqEstHours), 2));

                    // Add cells to row
                    tr.Cells.Add(labCount);
                    tr.Cells.Add(labType);
                    tr.Cells.Add(labTask);
                    tr.Cells.Add(labHours);
                    tr.Cells.Add(labUnPrice);
                    tr.Cells.Add(labExPrice);

                    // Update Labour Total
                    labourTotal += Convert.ToDecimal(w.workerType.wrkTypePrice * w.labourReq.lreqEstHours);
                    tblLabourSummary.Rows.Add(tr);
                    labourCount++;
                }
                //END PULL LABOUR TABLE

                //Start Summary Section
                bidTotal = labourTotal + materialsTotal;

                if (bidTotal > 0)
                {
                    if (labourTotal == 0)
                    {
                        //pie.InnerHtml = "100";
                    }
                    else
                    {
                        labourPercent = Math.Round((labourTotal / bidTotal) * 100);
                        if (labourPercent == 100)
                        {
                            labourPercent = 99.99m;
                        }
                        else if (labourPercent == 0)
                        {
                            labourPercent = 0.01m;
                        }
                    }
                }

                // Add totals to page
                headerLabourlTotal.InnerHtml = string.Format("{0:C}", Math.Round(Convert.ToDecimal(labourTotal), 2));
                headerMaterialTotal.InnerHtml = string.Format("{0:C}", Math.Round(Convert.ToDecimal(materialsTotal), 2));
                headerBidTotal.InnerHtml = string.Format("{0:C}", Math.Round(Convert.ToDecimal(bidTotal), 2));
            }
        }

        protected void btnEditBid_Click(object sender, EventArgs e)
        {
            //Turn on Save Button
            btnSaveBid.Visible = true;

            //Makes Fields Editable
            mdlTxtProjectName.ReadOnly = false;
            mdlTxtProjectSite.ReadOnly = false;
            mdlTxtNBDSales.ReadOnly = false;
            mdlTxtNBDDesigner.ReadOnly = false;

            //Show the edit Table and hide the Demo Table
            tblLabourSummary.Visible = false;
            editLabSummary.Visible = true;
            tblMaterialsSummary.Visible = false;
            editMatSummary.Visible = true;
        }

        protected void btnSaveBid_Click(object sender, EventArgs e)
        {
            //Turn off Save Button
            btnSaveBid.Visible = false;

            //Makes Fields Read Only
            mdlTxtProjectName.ReadOnly = true;
            mdlTxtProjectSite.ReadOnly = true;
            mdlTxtNBDSales.ReadOnly = true;
            mdlTxtNBDDesigner.ReadOnly = true;

            //Hide the Edit and show the summary table
            tblLabourSummary.Visible = true;
            editLabSummary.Visible = false;
            tblMaterialsSummary.Visible = true;
            editMatSummary.Visible = false;
        }
    }
}




