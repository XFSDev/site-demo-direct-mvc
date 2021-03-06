using System;
using System.Data;
using System.Web.UI.WebControls;
using Models;
using RadarSoft.RadarCube.Common;
using RadarSoft.RadarCube.Web;


namespace SamplesFactory
{
    public class SortingSample : BaseGridSample
    {
        public SortingSample(SamplesModel samplesModel) : base(samplesModel)
        {
        }

        protected override void InitLayout()
        {
            OlapAnalysis.BeginUpdate();

            THierarchy H = OlapAnalysis.Dimensions.FindHierarchyByDisplayName("Date");
            OlapAnalysis.PivotingFirst(H, TLayoutArea.laRow);
            TMeasure M = OlapAnalysis.Measures.FindByDisplayName("Sales");
            M.Visible = true;
            M = OlapAnalysis.Measures.FindByDisplayName("Quantity");
            M.Visible = true;

            OlapAnalysis.CellSet.Rebuild();

            IMemberCell imcell = OlapAnalysis.CellSet.Cells(0, 3) as IMemberCell;
            imcell.DrillAction(TPossibleDrillActions.esNextLevel);
            imcell = OlapAnalysis.CellSet.Cells(0, 8) as IMemberCell;
            imcell.DrillAction(TPossibleDrillActions.esNextLevel);

            OlapAnalysis.EndUpdate();

            H = OlapAnalysis.Dimensions.FindHierarchyByDisplayName("Date");
            OlapAnalysis.OnMemberSort += Grid_OnMemberSort;
            H.OverrideSortMethods = true;
            H.Sort();


        }

        void Grid_OnMemberSort(object Sender, TEventMemberSortArgs e)
        {
            //Get the original dates from the CubeMember
            DateTime D1 = e.MemberLow.CubeMember.BIDate;
            DateTime D2 = e.MemberHigh.CubeMember.BIDate;
            // Get the member level
            int L = e.MemberLow.Level.Index;
            if (L == 0)
            {
                // Simply compare years
                e.Result = D1.Year.CompareTo(D2.Year);
                // Provide reverse order for Years
                e.Result *= -1;
            }
            if (L == 1)
            {
                // Calculate quarters
                int Q1 = ((D1.Month - 1) / 3) + 1;
                int Q2 = ((D2.Month - 1) / 3) + 1;
                // Provide sorting taking into consideration years so that 1998-Q1 goes after 1997-Q4
                Q1 += D1.Year * 10;
                Q2 += D2.Year * 10;
                e.Result = Q1.CompareTo(Q2);
                // Provide reverse order for Quarters
                e.Result *= -1;
            }
            if (L == 2)
            {
                // Simply compare months
                e.Result = D1.Month.CompareTo(D2.Month);
                // Provide reverse order for moths
                e.Result *= -1;
            }
            // Invert the result if the descending order is active
            //   if (e.SortingMethod == TMembersSortType.msNameDesc) e.Result = -e.Result;
        }


    }
}