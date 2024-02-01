using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections;
using System.Xml.Linq;

namespace PartsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public class Inventory
        {
            public string Part { get; set; }
            public int In_Stock { get; set; }
            public int On_Order { get; set; }
            public bool Base { get; set; }
            public Inventory(string p, int i, int o, bool b)
            {
                Part = p; In_Stock = i; On_Order = o; Base = b;
            }
        }

        public class BasePart
        {
            public string Name { get; set; }
            public string Seller { get; set; }
            public string Sale_Link { get; set; }
            public float Price { get; set; }
            public string Notes { get; set; }
            public BasePart(string na, string s, string sl, float p, string no)
            {
                Name = na; Seller = s; Sale_Link = sl; Price = p; Notes = no;
            }
        }

        public class AssembledPart
        {
            public string Name { get; set; }
            public List<(string, int)> Components { get; set; }
            public string Procedure { get; set; }
            public string Notes { get; set; }
            public AssembledPart(string na, List<(string, int)> c, string p, string no)
            {
                Name = na; Components = c; Procedure = p; Notes = no;
            }
        }

        public class BuildDisplay
        {
            public string Comp { get; set; }
            public int Quant { get; set; }
            public int Stock { get; set; }
            public int Order { get; set; }

            public BuildDisplay(string comp, int quant, int stock, int order)
            {
                Comp = comp;
                Quant = quant;
                Stock = stock;
                Order = order;
            }
        }

        public List<Inventory> InventoryPartsList = new List<Inventory>();
        public List<AssembledPart> AssembledPartsList = new List<AssembledPart>();
        public List<AssembledPart> AvailablePartsList = new List<AssembledPart>();
        public List<AssembledPart> UnavailablePartsList = new List<AssembledPart>();
        public List<BasePart> BasePartsList = new List<BasePart>();
        public List<BuildDisplay> NewPartComponents = new List<BuildDisplay>();
        public List<BuildDisplay> ModPartComponents = new List<BuildDisplay>();

        public string conString = "Host=localhost;Username=postgres;Password=Passwone2;Database=postgres";

        public void UpdateInventory()
        {
            try
            {
                InventoryList.Items.Clear();
                InventoryPartsList.Clear();
                NewPartComp.Items.Clear();
                ModPartComp.Items.Clear();
                RemoveList.Items.Clear();
                ModList.Items.Clear();
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();

                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM Inventory", con);
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Inventory i = new Inventory(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetBoolean(3));
                    InventoryPartsList.Add(i);
                    InventoryList.Items.Add(reader.GetString(0));
                    NewPartComp.Items.Add(reader.GetString(0));
                    ModPartComp.Items.Add(reader.GetString(0));
                    RemoveList.Items.Add(reader.GetString(0));
                    ModList.Items.Add(reader.GetString(0));
                }

                reader.Close();
                con.Close();

                UpdateAssembled();
                UpdateBase();

            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }

        public void UpdateAssembled()
        {
            AssembledPartsList.Clear();
            AvailablePartsList.Clear();
            UnavailablePartsList.Clear();
            AssembleList.Items.Clear();
            try
            {
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();

                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM AssembledPart", con);
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string compList = reader.GetString(1);
                    string[] compPieces = compList.Split('|');
                    List<(string, int)> tempList = new List<(string, int)>();
                    for (int i = 0; i < compPieces.Length; i++)
                    {
                        string[] halves = compPieces[i].Split(':');
                        tempList.Add((halves[0], int.Parse(halves[1])));
                    }
                    AssembledPart a = new AssembledPart(reader.GetString(0), tempList, reader.GetString(2), reader.GetString(3));
                    AssembledPartsList.Add(a);
                    AssembleList.Items.Add(reader.GetString(0));
                }

                reader.Close();
                con.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            //For Assembled part
            for(int i = 0; i < AssembledPartsList.Count; i++)
            {
                bool missing = false;
                //For Component of Assembled part
                for (int j = 0; j < AssembledPartsList[i].Components.Count; j++)
                {
                    //Match Component with Inventory
                    for(int k = 0; k < InventoryPartsList.Count; k++)
                    {
                        if (AssembledPartsList[i].Components[j].Item1 == InventoryPartsList[k].Part)
                        {
                            //Check Components Required against In Stock
                            if (InventoryPartsList[k].In_Stock < AssembledPartsList[i].Components[j].Item2)
                            {
                                //we are missing components.
                                missing = true;
                                UnavailablePartsList.Add(AssembledPartsList[i]);
                                break;
                            }
                        }
                    }
                    if (missing)
                    {
                        break;
                    }
                }
                if (!missing)
                {
                    AvailablePartsList.Add(AssembledPartsList[i]);
                }
            }
        }

        public void UpdateBase()
        {
            BasePartsList.Clear();
            //BaseList.Items.Clear();
            try
            {
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();

                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM BasePart", con);
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    BasePart b = new BasePart(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetFloat(3), reader.GetString(4));
                    BasePartsList.Add(b);
                    //BaseList.Items.Add(reader.GetString(0));
                }

                reader.Close();

                BaseList.Items.Clear();
                command = new NpgsqlCommand("SELECT * FROM Inventory", con);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetBoolean(3))
                    {
                        BaseList.Items.Add(reader.GetString(0));
                    }
                }

                reader.Close();
                con.Close();

            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            string rawPath = System.IO.Path.GetFullPath(".");
            string[] broken = rawPath.Split('\\');
            string masterPath = broken[0];
            for(int i = 0;  i < broken.Length - 1; i++)
            {
                masterPath += "\\" + broken[i];
            }

            AssembledPartsList.Clear();

            try
            {
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();

                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM Inventory", con);
                NpgsqlDataReader reader = command.ExecuteReader();
                BaseList.Items.Clear();
                InventoryList.Items.Clear();
                while(reader.Read())
                {
                    Inventory i = new Inventory(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetBoolean(3));
                    InventoryPartsList.Add(i);
                    if (reader.GetBoolean(3))
                    {
                        BaseList.Items.Add(reader.GetString(0));
                    }
                    InventoryList.Items.Add(reader.GetString(0));
                }

                reader.Close();
                AssembleList.Items.Clear();
                command = new NpgsqlCommand("SELECT * FROM AssembledPart", con);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string compList = reader.GetString(1);
                    string[] compPieces = compList.Split('|');
                    List<(string, int)> tempList = new List<(string, int)> ();

                    for(int i = 0; i < compPieces.Length; i++)
                    {
                        string[] halves = compPieces[i].Split(':');
                        tempList.Add((halves[0], int.Parse(halves[1])));
                    }
                    AssembledPartsList.Add(new AssembledPart(reader.GetString(0), tempList, reader.GetString(2), reader.GetString(3)));
                    AssembleList.Items.Add(reader.GetString(0));
                }

                reader.Close();
                command = new NpgsqlCommand("Select * FROM BasePart", con);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    BasePartsList.Add(new BasePart(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetFloat(3), reader.GetString(4)));
                }

                reader.Close();

                con.Close();
            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            UpdateAssembled();

            AvailableList.Items.Clear();
            for (int i = 0; i < AvailablePartsList.Count; i++)
            {
                //MessageBox.Show(AvailablePartsList[i].Name);
                AvailableList.Items.Add(AvailablePartsList[i].Name);
            }

            UnavailableList.Items.Clear();
            for(int i = 0; i < UnavailablePartsList.Count; i++)
            {
                //MessageBox.Show(UnavailablePartsList[i].Name);
                UnavailableList.Items.Add(UnavailablePartsList[i].Name);
            }

            Stock.ItemsSource = InventoryPartsList;

            NewPartComp.Items.Clear();
            ModPartComp.Items.Clear();
            RemoveList.Items.Clear();
            ModList.Items.Clear();
            for (int i = 0; i < InventoryPartsList.Count; i++)
            {
                NewPartComp.Items.Add(InventoryPartsList[i].Part);
                ModPartComp.Items.Add(InventoryPartsList[i].Part);
                RemoveList.Items.Add(InventoryPartsList[i].Part);
                ModList.Items.Add(InventoryPartsList[i].Part);
            }

            BaseGrid.IsEnabled = false;
            ModBaseGrid.IsEnabled = false;
            ModAssembledGrid.IsEnabled = false;

            NewPartCompPieces.ItemsSource = NewPartComponents;
            ModPartCompPieces.ItemsSource = ModPartComponents;
        }
        

        private void AvailableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(AvailableList.SelectedIndex == -1)
            {
                return;
            }
            UnavailableList.SelectedIndex = -1;
            string name = AvailableList.SelectedItem.ToString();
            AssembledPart part = new AssembledPart("", new List<(string, int)>(), "", "");
            for(int i = 0; i < AvailablePartsList.Count; i++)
            {
                if (AvailablePartsList[i].Name == name)
                {
                    part = AvailablePartsList[i];
                    break;
                }
            }
            Procedure.Text = part.Procedure;
            List<BuildDisplay> d1 = new List<BuildDisplay>();
            for (int i = 0; i < part.Components.Count; i++)
            {
                int s = 0;
                int o = 0;
                for(int j = 0; j < InventoryPartsList.Count; j++)
                {
                    if (part.Components[i].Item1 == InventoryPartsList[j].Part)
                    {
                        s = InventoryPartsList[j].In_Stock;
                        o = InventoryPartsList[j].On_Order;
                        break;
                    }
                }
                d1.Add(new BuildDisplay(part.Components[i].Item1, part.Components[i].Item2, s, o));
            }
            Comps.ItemsSource = d1;
            Notes.Text = part.Notes;
        }

        private void UnavailableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(UnavailableList.SelectedIndex == -1)
            {
                return;
            }
            AvailableList.SelectedIndex = -1;
            string name = UnavailableList.SelectedItem.ToString();
            AssembledPart part = new AssembledPart("", new List<(string, int)>(), "", "");
            for (int i = 0; i < UnavailablePartsList.Count; i++)
            {
                if (UnavailablePartsList[i].Name == name)
                {
                    part = UnavailablePartsList[i];
                    break;
                }
            }
            Procedure.Text = part.Procedure;
            List<BuildDisplay> d1 = new List<BuildDisplay>();
            for (int i = 0; i < part.Components.Count; i++)
            {
                int s = 0;
                int o = 0;
                for (int j = 0; j < InventoryPartsList.Count; j++)
                {
                    if (part.Components[i].Item1 == InventoryPartsList[j].Part)
                    {
                        s = InventoryPartsList[j].In_Stock;
                        o = InventoryPartsList[j].On_Order;
                        break;
                    }
                }
                d1.Add(new BuildDisplay(part.Components[i].Item1, part.Components[i].Item2, s, o));
            }
            Comps.ItemsSource = d1;
            Notes.Text = part.Notes;
        }

        private void BaseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(BaseList.SelectedIndex == -1)
            {
                return;
            }
            SellerList.SelectedIndex = -1;
            Price.Text = "TextBlock";
            SellNotes.Text = "TextBlock";
            string name = BaseList.SelectedItem.ToString();
            List<string> sellers = new List<string>();
            SellerList.Items.Clear();
            SellNotes.Text = "";
            SaleLinkText.Text = "";
            for (int i = 0; i < BasePartsList.Count; i++)
            {
                if (BasePartsList[i].Name == name)
                {
                    SellerList.Items.Add(BasePartsList[i].Seller);
                }
            }
        }

        private void SellerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SellerList.SelectedIndex == -1)
            {
                return;
            }
            string pname = BaseList.SelectedItem.ToString();
            string sname = SellerList.SelectedItem.ToString();
            for(int i = 0; i < BasePartsList.Count; i++)
            {
                //MessageBox.Show($"{BasePartsList[i].Name} = {pname} | {BasePartsList[i].Seller} = {sname}");
                if (BasePartsList[i].Name == pname && BasePartsList[i].Seller == sname)
                {
                    //MessageBox.Show("Mark");
                    Price.Text = "$" + BasePartsList[i].Price.ToString();
                    SellNotes.Text = BasePartsList[i].Notes;
                    SaleLinkText.Text = BasePartsList[i].Sale_Link;
                    SaleHyperLink.NavigateUri = new Uri(BasePartsList[i].Sale_Link);
                }
            }
        }

        private void InventoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(InventoryList.SelectedIndex == -1) { return; }
            string name = InventoryList.SelectedItem.ToString();
            bool b = false;
            for(int i = 0; i < InventoryPartsList.Count; i++)
            {
                if(name == InventoryPartsList[i].Part)
                {
                    InvStock.Text = InventoryPartsList[i].In_Stock.ToString();
                    InvOrder.Text = InventoryPartsList[i].On_Order.ToString();
                    b = InventoryPartsList[i].Base;
                    break;
                }
            }
            InventoryNotes.Text = "";
            int count = 0;
            if (b)
            {
                for(int i = 0; i < BasePartsList.Count; i++)
                {
                    if (BasePartsList[i].Name == name)
                    {
                        count++;
                        InventoryNotes.Text += $"{count}: " + BasePartsList[i].Notes + "\n";
                    }
                }
            } else
            {
                for (int i = 0; i < AssembledPartsList.Count; i++)
                {
                    if (AssembledPartsList[i].Name == name)
                    {
                        count++;
                        InventoryNotes.Text += $"{count}: " + AssembledPartsList[i].Notes + "\n";
                    }
                }
            }
        }

        private void PreviewInventoryTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex r = new Regex(@"^-?[0-9]*$");
            e.Handled = !r.IsMatch(e.Text);
        }

        private void PreviewAssembleTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex r = new Regex(@"^[0-9]*$");
            e.Handled = !r.IsMatch(e.Text);
        }

        private void PreviewPriceTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex r = new Regex(@"^[0-9]*.?[0-9]*$");
            e.Handled = !r.IsMatch(e.Text);
        }

        private void PreviewNameTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex r = new Regex(@":|\|");
            e.Handled = r.IsMatch(e.Text);
        }

        private void ChangeStock_Click(object sender, RoutedEventArgs e)
        {
            if(InventoryList.SelectedIndex == -1)
            {
                MessageBox.Show("Select part to add or subtract stock.");
                return;
            }
            string name = InventoryList.SelectedItem.ToString();
            string na = ""; int qu = 0;
            for(int i = 0; i < InventoryPartsList.Count; i++)
            {
                if (InventoryPartsList[i].Part == name)
                {
                    na = InventoryPartsList[i].Part;
                    qu = InventoryPartsList[i].In_Stock;
                    break;
                }
            }
            int m = 0;
            int f = 0;
            try
            {
                m = int.Parse(ModStock.Text);
                f = m + qu;
                if(f < 0)
                {
                    MessageBox.Show("Not enough stock to subtract.");
                    return;
                }
            } catch
            {
                MessageBox.Show("Invalid value entered.");
                return;
            }
            try
            {
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();
                //NpgsqlCommand command = new NpgsqlCommand($"UPDATE Inventory SET Part = '{na}', In_Stock = {f} WHERE Part = '{na}'", con);
                NpgsqlCommand command = new NpgsqlCommand("UPDATE Inventory SET Part = ($1), In_Stock = ($2) WHERE Part = ($1)", con)
                {
                    Parameters =
                    {
                        new NpgsqlParameter() { Value = na },
                        new NpgsqlParameter() { Value = f }
                    }
                };
                command.ExecuteNonQuery();
                con.Close();

                InvStock.Text = f.ToString();
                UpdateInventory();
                MessageBox.Show("Stock Added.");

            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void ChangeOrder_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryList.SelectedIndex == -1)
            {
                MessageBox.Show("Select part to add or subtract stock.");
                return;
            }
            string name = InventoryList.SelectedItem.ToString();
            string na = ""; int qu = 0;
            for (int i = 0; i < InventoryPartsList.Count; i++)
            {
                if (InventoryPartsList[i].Part == name)
                {
                    na = InventoryPartsList[i].Part;
                    qu = InventoryPartsList[i].On_Order;
                    break;
                }
            }
            int m = 0;
            int f = 0;
            try
            {
                m = int.Parse(ModOrder.Text);
                f = m + qu;
                if (f < 0)
                {
                    MessageBox.Show("Not enough orders to subtract.");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Invalid value entered.");
                return;
            }
            try
            {
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();
                //NpgsqlCommand command = new NpgsqlCommand($"UPDATE Inventory SET Part = '{na}', In_Stock = {f} WHERE Part = '{na}'", con);
                NpgsqlCommand command = new NpgsqlCommand("UPDATE Inventory SET Part = ($1), On_Order = ($2) WHERE Part = ($1)", con)
                {
                    Parameters =
                    {
                        new NpgsqlParameter() { Value = na },
                        new NpgsqlParameter() { Value = f }
                    }
                };
                command.ExecuteNonQuery();
                con.Close();

                InvOrder.Text = f.ToString();
                UpdateInventory();
                MessageBox.Show("Order Added.");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void ChangeReceive_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryList.SelectedIndex == -1)
            {
                MessageBox.Show("Select part to add or subtract stock.");
                return;
            }
            string name = InventoryList.SelectedItem.ToString();
            string na = ""; int ins = 0; int oo = 0;
            for (int i = 0; i < InventoryPartsList.Count; i++)
            {
                if (InventoryPartsList[i].Part == name)
                {
                    na = InventoryPartsList[i].Part;
                    ins = InventoryPartsList[i].In_Stock;
                    oo = InventoryPartsList[i].On_Order;
                    break;
                }
            }
            int m = 0;
            int f1 = 0;
            int f2 = 0;
            try
            {
                m = int.Parse(ModReceive.Text);
                f1 = m + ins;
                f2 = oo - m;
                if (f1 < 0)
                {
                    MessageBox.Show("Not enough stock to subtract.");
                    return;
                }
                if(f2 < 0)
                {
                    MessageBox.Show("Not enough orders to receive.");
                }
            }
            catch
            {
                MessageBox.Show("Invalid value entered.");
                return;
            }
            try
            {
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();
                //NpgsqlCommand command = new NpgsqlCommand($"UPDATE Inventory SET Part = '{na}', In_Stock = {f} WHERE Part = '{na}'", con);
                NpgsqlCommand command = new NpgsqlCommand("UPDATE Inventory SET Part = ($1), In_Stock = ($2), On_Order = ($3) WHERE Part = ($1)", con)
                {
                    Parameters =
                    {
                        new NpgsqlParameter() { Value = na },
                        new NpgsqlParameter() { Value = f1 },
                        new NpgsqlParameter() { Value = f2 }
                    }
                };
                command.ExecuteNonQuery();
                con.Close();

                InvStock.Text = f1.ToString();
                InvOrder.Text = f2.ToString();
                UpdateInventory();
                MessageBox.Show("Order Received.");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void HomeTabSelected(object sender, RoutedEventArgs e)
        {
            AvailableList.Items.Clear();
            for (int i = 0; i < AvailablePartsList.Count; i++)
            {
                AvailableList.Items.Add(AvailablePartsList[i].Name);
            }

            UnavailableList.Items.Clear();
            for (int i = 0; i < UnavailablePartsList.Count; i++)
            {
                UnavailableList.Items.Add(UnavailablePartsList[i].Name);
            }

            Stock.Items.Refresh();
            Stock.ItemsSource = InventoryPartsList;
        }

        private void AssembleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(AssembleList.SelectedIndex == -1)
            {
                return;
            }
            string name = AssembleList.SelectedItem.ToString();
            AssembledPart part = new AssembledPart("", new List<(string, int)>(), "", "");
            for (int i = 0; i < AssembledPartsList.Count; i++)
            {
                if (AssembledPartsList[i].Name == name)
                {
                    part = AssembledPartsList[i];
                    break;
                }
            }

            int aStock = 0; int aOrder = 0;
            for(int i = 0; i < InventoryPartsList.Count; i++)
            {
                if (InventoryPartsList[i].Part == name)
                {
                    aStock = InventoryPartsList[i].In_Stock;
                    aOrder = InventoryPartsList[i].On_Order;
                    break;
                }
            }
            
            List<BuildDisplay> d1 = new List<BuildDisplay>();
            for (int i = 0; i < part.Components.Count; i++)
            {
                int s = 0;
                //int o = 0;
                int missing = 0;
                for (int j = 0; j < InventoryPartsList.Count; j++)
                {
                    if (part.Components[i].Item1 == InventoryPartsList[j].Part)
                    {
                        s = InventoryPartsList[j].In_Stock;
                        //o = InventoryPartsList[j].On_Order;
                        missing = part.Components[i].Item2 - s;
                        break;
                    }
                }
                d1.Add(new BuildDisplay(part.Components[i].Item1, part.Components[i].Item2, s, missing));
            }
            AssembleComps.ItemsSource = d1;
            AssembleStock.Text = aStock.ToString();
            AssembleOrder.Text = aOrder.ToString();

            calcStock.Clear();

            Dictionary<string, int> inv = new Dictionary<string, int>();
            for (int i = 0; i < InventoryPartsList.Count; i++)
            {
                inv.Add(InventoryPartsList[i].Part, InventoryPartsList[i].In_Stock);
                calcStock.Add(InventoryPartsList[i].Part, InventoryPartsList[i].In_Stock);
            }

            Dictionary<string, int> raw = Breakdown(name, 1);

            List<BuildDisplay> d2 = new List<BuildDisplay>();
            foreach (KeyValuePair<string, int> kvp in raw)
            {
                d2.Add(new BuildDisplay(kvp.Key, kvp.Value, inv[kvp.Key], 0));
            }
            AssembleNeed.ItemsSource = d2;
        }

        public Dictionary<string, int> calcStock = new Dictionary<string, int>();

        public Dictionary<string, int> Breakdown(string name, int req)
        {

            Dictionary<string, int> x = new Dictionary<string, int>();
            AssembledPart part = new AssembledPart("", new List<(string, int)> { ("", 0) }, "", "");
            //find part
            for(int i = 0; i < AssembledPartsList.Count; i++)
            {
                if(name == AssembledPartsList[i].Name)
                {
                    part = AssembledPartsList[i];
                    break;
                }
            }
            //for component of part
            for(int i = 0; i < part.Components.Count; i++)
            {
                //if we have enough in stock (for all required).
                if (calcStock[part.Components[i].Item1] >= (part.Components[i].Item2) * req)
                {
                    //subtract from stock
                    calcStock[part.Components[i].Item1] -= (part.Components[i].Item2 * req);
                } else
                {
                    //Match component with Inventory
                    for(int j = 0; j < InventoryPartsList.Count; j++)
                    {
                        if (part.Components[i].Item1 == InventoryPartsList[j].Part)
                        {
                            //check if part is base
                            if (InventoryPartsList[j].Base)
                            {
                                int need = part.Components[i].Item2 * req;
                                //Use what stock we do have and record the difference.
                                if (x.ContainsKey(part.Components[i].Item1))
                                {
                                    x[part.Components[i].Item1] += (need - calcStock[part.Components[i].Item1]);
                                } else
                                {
                                    x.Add(part.Components[i].Item1, (need - calcStock[part.Components[i].Item1]));
                                }
                                calcStock[part.Components[i].Item1] = 0;
                            } else
                            {
                                int dif = (part.Components[i].Item2 * req) - calcStock[part.Components[i].Item1];
                                calcStock[part.Components[i].Item1] = 0;
                                Dictionary<string, int> temp = Breakdown(part.Components[i].Item1, dif);
                                //Add recursion in to our list
                                foreach(KeyValuePair<string, int> kvp in temp)
                                {
                                    if (x.ContainsKey(kvp.Key))
                                    {
                                        x[kvp.Key] += kvp.Value;
                                    } else
                                    {
                                        x.Add(kvp.Key, kvp.Value);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            return x;
        }

        /*
        public List<(string, int)> Breakdown(string name)
        {
            AssembledPart p = new AssembledPart("", new List<(string, int)>{ ("",0) }, "", "");
            Dictionary<string, int> x = new Dictionary<string, int>();
            List<(string, int)> y = new List<(string, int)>();
            for (int i = 0; i < AssembledPartsList.Count; i++)
            {
                if(name == AssembledPartsList[i].Name)
                {
                    p = AssembledPartsList[i];
                    break;
                }
            }

            for(int i=0; i < p.Components.Count; i++)
            {
                for(int j=0; j < InventoryPartsList.Count; j++)
                {
                    if (p.Components[i].Item1 == InventoryPartsList[j].Part)
                    {
                        if (InventoryPartsList[j].Base)
                        {
                            if (x.ContainsKey(p.Components[i].Item1))
                            {
                                x[p.Components[i].Item1] += p.Components[i].Item2;
                            } else
                            {
                                x.Add(p.Components[i].Item1, p.Components[i].Item2);
                            }
                        } else
                        {
                            y = Breakdown(p.Components[i].Item1);
                            for(int k=0; k<y.Count; k++)
                            {
                                if (x.ContainsKey(y[k].Item1))
                                {
                                    x[y[k].Item1] += (y[k].Item2 * p.Components[i].Item2);
                                } else
                                {
                                    x.Add(y[k].Item1, (y[k].Item2 * p.Components[i].Item2));
                                }
                            }
                        }
                        break;
                    }
                }
            }

            List<(string, int)> z = new List<(string, int)>();
            foreach(KeyValuePair<string, int> kvp in x)
            {
                z.Add((kvp.Key, kvp.Value));
            }

            return z;
        }
        */

        private void AssemblePart_Click(object sender, RoutedEventArgs e)
        {
            if (AssembleList.SelectedIndex == -1)
            {
                MessageBox.Show("Select part to add or subtract stock.");
                return;
            }
            string na = AssembleList.SelectedItem.ToString(); List<(string, int)> c = new List<(string, int)>();
            for(int i = 0; i < AssembledPartsList.Count; i++)
            {
                if (AssembledPartsList[i].Name == na)
                {
                    c = AssembledPartsList[i].Components;
                    break;
                }
            }
            int aStock = 0;
            for(int i = 0; i < InventoryPartsList.Count; i++)
            {
                if (InventoryPartsList[i].Part == na)
                {
                    aStock = InventoryPartsList[i].In_Stock;
                    break;
                }
            }
            int m = 0; int f = 0;
            List<int> bStock = new List<int>();
            try
            {
                m = int.Parse(AssembleNum.Text);
                for(int i = 0; i < c.Count; i++)
                {
                    for(int j = 0; j < InventoryPartsList.Count; j++)
                    {
                        if (InventoryPartsList[j].Part == c[i].Item1)
                        {
                            bStock.Add(InventoryPartsList[j].In_Stock);
                            break;
                        }
                    }
                    f = m * c[i].Item2;
                    if (f > bStock[i])
                    {
                        MessageBox.Show("Not enough stock to subtract.");
                        return;
                    }
                }
                
            } catch
            {
                MessageBox.Show("Invalid value entered."); 
                return;
            }

            try
            {
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();
                //NpgsqlCommand command = new NpgsqlCommand($"UPDATE Inventory SET Part = '{na}', In_Stock = {aStock+m} WHERE Part = '{na}'", con);
                NpgsqlCommand command = new NpgsqlCommand("UPDATE Inventory SET Part = ($1), In_Stock = ($2) WHERE Part = ($1)", con)
                {
                    Parameters =
                    {
                        new NpgsqlParameter { Value = na },
                        new NpgsqlParameter { Value = aStock+m }
                    }
                };
                command.ExecuteNonQuery();
                for (int i = 0; i < c.Count; i++)
                {
                    //command = new NpgsqlCommand($"UPDATE Inventory SET Part = '{c[i].Item1}', In_Stock = {bStock[i] - (m * c[i].Item2)} WHERE Part = '{c[i].Item1}'", con);
                    command = new NpgsqlCommand("UPDATE Inventory SET Part = ($1), In_Stock = ($2) WHERE Part = ($3)", con)
                    {
                        Parameters =
                        {
                            new NpgsqlParameter { Value = c[i].Item1 },
                            new NpgsqlParameter { Value = bStock[i] - (m * c[i].Item2) },
                            new NpgsqlParameter { Value = c[i].Item1 }
                        }
                    };
                    command.ExecuteNonQuery();
                }

                con.Close();

                InvStock.Text = f.ToString();
                UpdateInventory();

                //Update In-Stock for components list.
                List<BuildDisplay> d1 = new List<BuildDisplay>();
                for (int i = 0; i < c.Count; i++)
                {
                    int s = 0;
                    int o = 0;
                    for (int j = 0; j < InventoryPartsList.Count; j++)
                    {
                        if (c[i].Item1 == InventoryPartsList[j].Part)
                        {
                            s = InventoryPartsList[j].In_Stock;
                            o = InventoryPartsList[j].On_Order;
                            break;
                        }
                    }
                    d1.Add(new BuildDisplay(c[i].Item1, c[i].Item2, s, o));
                }
                AssembleComps.ItemsSource = d1;
                AssembleStock.Text = (aStock + m).ToString();

                MessageBox.Show("Part(s) Assembled.");

            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void AssembleTabSelected(object sender, RoutedEventArgs e)
        {
            AssembleList.SelectedIndex = -1;
            AssembleComps.ItemsSource = null;
            AssembleStock.Text = "TextBlock";
            AssembleOrder.Text = "TextBlock";
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BaseGrid.IsEnabled = true;
            AssembledGrid.IsEnabled = false;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            BaseGrid.IsEnabled = false;
            AssembledGrid.IsEnabled = true;
        }

        private void NewPartAdd_Click(object sender, RoutedEventArgs e)
        {
            int x = 0;
            if(NewPartComp.SelectedIndex == -1) { MessageBox.Show("Select component to add."); return; }
            try { x = int.Parse(NewPartCompAmount.Text); } catch { MessageBox.Show("Invalid value entered."); return; }
            NewPartComponents.Add(new BuildDisplay(NewPartComp.SelectedItem.ToString(), x, 0, 0));
            NewPartCompPieces.Items.Refresh();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            NewPartComponents.Clear();
            NewPartCompPieces.Items.Refresh();
        }

        private void CreatePart_Click(object sender, RoutedEventArgs e)
        {
            Regex re = new Regex(@":|\|");
            if (NewPartName.Text == "" || re.IsMatch(NewPartName.Text)) { MessageBox.Show("Valid part name required."); return; }
            TextRange tr = new TextRange(NewPartNotes.Document.ContentStart, NewPartNotes.Document.ContentEnd);
            if ((bool)BaseCheck.IsChecked)
            {
                if(NewPartSeller.Text == "") { MessageBox.Show("Part seller required."); return; }
                float x = 0f;
                try { x = float.Parse(NewPartPrice.Text); } catch { MessageBox.Show("Invalid part price entered."); return; }

                //Add to Inventory (if doesn't already exist)
                //Add to BaseParts
                try
                {
                    NpgsqlConnection con = new NpgsqlConnection(conString);
                    con.Open();
                    //NpgsqlCommand command = new NpgsqlCommand($"Insert Into BaseParts Values ('{NewPartName.Text}', '{NewPartSeller.Text}', {x}, '{tr.Text}')", con);
                    NpgsqlCommand command = new NpgsqlCommand("Insert Into BasePart Values (($1), ($2), ($3), ($4))", con)
                    {
                        Parameters =
                        {
                            new NpgsqlParameter { Value = NewPartName.Text },
                            new NpgsqlParameter { Value = NewPartSeller.Text },
                            new NpgsqlParameter { Value = x },
                            new NpgsqlParameter { Value = tr.Text }
                        }
                    };
                    command.ExecuteNonQuery();

                    //command = new NpgsqlCommand($"SELECT COUNT(*) FROM Inventory WHERE Part = '{NewPartName.Text}'");
                    command = new NpgsqlCommand("SELECT COUNT(*) FROM Inventory WHERE Part = ($1)", con)
                    {
                        Parameters =
                        {
                            new NpgsqlParameter { Value = NewPartName.Text }
                        }
                    };
                    NpgsqlDataReader reader = command.ExecuteReader();
                    int t = 0;
                    while(reader.Read())
                    {
                        t = reader.GetInt32(0);
                    }
                    reader.Close();
                    if(t == 0)
                    {
                        //command = new NpgsqlCommand($"Inesert Into Inventory Values ('{NewPartName.Text}', 0, true)");
                        command = new NpgsqlCommand("Insert Into Inventory Values (($1), 0, true)", con)
                        {
                            Parameters =
                            {
                                new NpgsqlParameter { Value = NewPartName.Text }
                            }
                        };
                        command.ExecuteNonQuery();
                    }

                    con.Close();

                    UpdateInventory();
                    MessageBox.Show("Part Created.");
                    return;

                } catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString()); return;
                }

            } else
            {
                if (!NewPartComponents.Any()) { MessageBox.Show("Assembled part requires components"); return; }
                if(NewPartFile.Text == "") { MessageBox.Show("Part prodedure form required."); return; }

                //Parse components.
                //Add to Inventory (if doesn't already exist)
                //Add to AssembledParts

                string c = $"{NewPartComponents[0].Comp}:{NewPartComponents[0].Quant}";
                for(int i=1; i < NewPartComponents.Count; i++) 
                {
                    c += $"|{NewPartComponents[i].Comp}:{NewPartComponents[i].Quant}";
                }
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();
                //NpgsqlCommand command = new NpgsqlCommand($"Insert Into AssembledParts Values ('{NewPartName.Text}', '{c}', {NewPartFile}, '{tr.Text}')", con);
                NpgsqlCommand command = new NpgsqlCommand("Insert Into AssembledPart Values (($1), ($2), ($3), ($4))", con)
                {
                    Parameters =
                    {
                        new NpgsqlParameter { Value = NewPartName.Text },
                        new NpgsqlParameter { Value = c },
                        new NpgsqlParameter { Value = NewPartFile.Text },
                        new NpgsqlParameter { Value = tr.Text }
                    }
                };
                command.ExecuteNonQuery();

                //command = new NpgsqlCommand($"SELECT COUNT(*) FROM Inventory WHERE Part = '{NewPartName.Text}'");
                command = new NpgsqlCommand("SELECT COUNT(*) FROM Inventory WHERE Part = ($1)", con)
                {
                    Parameters =
                        {
                            new NpgsqlParameter { Value = NewPartName.Text }
                        }
                };
                NpgsqlDataReader reader = command.ExecuteReader();
                int t = 0;
                while (reader.Read())
                {
                    t = reader.GetInt32(0);
                }
                reader.Close();
                if (t == 0)
                {
                    //command = new NpgsqlCommand($"Inesert Into Inventory Values ('{NewPartName.Text}', 0, false)");
                    command = new NpgsqlCommand("Insert Into Inventory Values (($1), 0, false)", con)
                    {
                        Parameters =
                            {
                                new NpgsqlParameter { Value = NewPartName.Text }
                            }
                    };
                    command.ExecuteNonQuery();
                }

                con.Close();

                UpdateInventory();
                MessageBox.Show("Part Created.");
                return;
            }
        }

        private void RemovePart_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxResult r;

            r = MessageBox.Show("Are you sure? Part will be permanently removed.", "Remove Part?", button, MessageBoxImage.Warning, MessageBoxResult.OK);
            if (r == MessageBoxResult.OK)
            {
                string name = RemoveList.SelectedItem.ToString();
                bool b = false;
                for(int i = 0; i < InventoryPartsList.Count; i++)
                {
                    if (InventoryPartsList[i].Part == name)
                    {
                        b = InventoryPartsList[i].Base;
                    }
                }
                try
                {
                    NpgsqlConnection con = new NpgsqlConnection(conString);
                    con.Open();
                    NpgsqlCommand command = new NpgsqlCommand("DELETE FROM Inventory WHERE Part = ($1)", con)
                    {
                        Parameters =
                        {
                            new NpgsqlParameter { Value = name }
                        }
                    };
                    command.ExecuteNonQuery();
                    if (b)
                    {
                        command = new NpgsqlCommand("DELETE FROM BasePart WHERE Part = ($1)", con)
                        {
                            Parameters =
                            {
                                new NpgsqlParameter { Value = name }
                            }
                        };
                        command.ExecuteNonQuery();
                    } else
                    {
                        command = new NpgsqlCommand("DELETE FROM AssembledPart WHERE Part = ($1)", con)
                        {
                            Parameters =
                            {
                                new NpgsqlParameter { Value = name }
                            }
                        };
                        command.ExecuteNonQuery();
                    }
                    con.Close();
                    MessageBox.Show("Part Removed.");
                    UpdateInventory();
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            } else
            {
                return;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ModList.SelectedIndex == -1)
            {
                return;
            }

            TextRange tr = new TextRange(ModBaseNotes.Document.ContentStart, ModBaseNotes.Document.ContentEnd);
            ModSeller.Text = "";
            ModPrice.Text = "";
            tr.Text = "";
            ModSaleLink.Text = "";

            ModForm.Text = "";
            tr = new TextRange(ModAssembledNotes.Document.ContentStart, ModAssembledNotes.Document.ContentEnd);
            tr.Text = "";

            string name = ModList.SelectedItem.ToString();
            bool b = false;
            for (int i = 0; i < InventoryPartsList.Count; i++)
            {
                if(name == InventoryPartsList[i].Part)
                {
                    b = InventoryPartsList[i].Base;
                    break;
                }
            }
            if (b)
            {
                ModBaseGrid.IsEnabled = true;
                ModAssembledGrid.IsEnabled = false;
                ModSellerList.SelectedIndex = -1;
                ModSellerList.Items.Clear();
                for (int i = 0; i < BasePartsList.Count; i++)
                {
                    if (BasePartsList[i].Name == name)
                    {
                        ModSellerList.Items.Add(BasePartsList[i].Seller);
                    }
                }
            } else
            {
                ModAssembledGrid.IsEnabled = true;
                ModBaseGrid.IsEnabled = false;
                tr = new TextRange(ModAssembledNotes.Document.ContentStart, ModAssembledNotes.Document.ContentEnd);
                for(int i = 0; i < AssembledPartsList.Count; i++)
                {
                    if (AssembledPartsList[i].Name == name)
                    {
                        tr.Text = AssembledPartsList[i].Notes;
                        ModForm.Text = AssembledPartsList[i].Procedure;
                        break;
                    }
                }

            }
        }

        private void ModSellerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ModSellerList.SelectedIndex == -1)
            {
                return;
            }
            string pname = ModList.SelectedItem.ToString();
            string sname = ModSellerList.SelectedItem.ToString();
            for (int i = 0; i < BasePartsList.Count; i++)
            {
                if (BasePartsList[i].Name == pname && BasePartsList[i].Seller == sname)
                {
                    TextRange tr = new TextRange(ModBaseNotes.Document.ContentStart, ModBaseNotes.Document.ContentEnd);
                    ModSeller.Text = sname;
                    ModPrice.Text = BasePartsList[i].Price.ToString();
                    tr.Text = BasePartsList[i].Notes;
                    ModSaleLink.Text = BasePartsList[i].Sale_Link;
                }
            }
        }

        private void ModPartAdd_Click(object sender, RoutedEventArgs e)
        {
            int x = 0;
            if (ModPartComp.SelectedIndex == -1) { MessageBox.Show("Select component to add."); return; }
            try { x = int.Parse(ModPartCompAmount.Text); } catch { MessageBox.Show("Invalid value entered."); return; }
            ModPartComponents.Add(new BuildDisplay(ModPartComp.SelectedItem.ToString(), x, 0, 0));
            ModPartCompPieces.Items.Refresh();
        }

        private void ClearMod_Click(object sender, RoutedEventArgs e)
        {
            ModPartComponents.Clear();
            ModPartCompPieces.Items.Refresh();
        }

        private void ModifyPart_Click(object sender, RoutedEventArgs e)
        {
            if(ModList.SelectedIndex == -1) { MessageBox.Show("Select Part to Modify"); return; }
            string name = ModList.SelectedItem.ToString();
            bool b = false;
            for(int i = 0; i < InventoryPartsList.Count; i++)
            {
                if(name == InventoryPartsList[i].Part)
                {
                    b = InventoryPartsList[i].Base;
                    break;
                }
            }
            try
            {
                NpgsqlConnection con = new NpgsqlConnection(conString);
                con.Open();
                NpgsqlCommand command;
                if (b)
                {
                    TextRange tr = new TextRange(ModBaseNotes.Document.ContentStart, ModBaseNotes.Document.ContentEnd);
                    command = new NpgsqlCommand("UPDATE BasePart SET Seller = ($1), Sale_Link = ($2), Price = ($3), Notes = ($4) WHERE Part = ($5) AND Seller = ($6)", con)
                    {
                        Parameters =
                            {
                                new NpgsqlParameter { Value = ModSeller.Text },
                                new NpgsqlParameter { Value = ModSaleLink.Text },
                                new NpgsqlParameter { Value = ModPrice.Text },
                                new NpgsqlParameter { Value = tr.Text },
                                new NpgsqlParameter { Value = name },
                                new NpgsqlParameter { Value = SellerList.SelectedItem.ToString() }
                            }
                    };
                    command.ExecuteNonQuery();
                }
                else
                {
                    if (!ModPartComponents.Any()) { MessageBox.Show("Assembled part requires components."); con.Close(); return; }
                    TextRange tr = new TextRange(ModAssembledNotes.Document.ContentStart, ModAssembledNotes.Document.ContentEnd);
                    string c = $"{ModPartComponents[0].Comp}:{ModPartComponents[0].Quant}";
                    for (int i = 1; i < ModPartComponents.Count; i++)
                    {
                        c += $"|{ModPartComponents[i].Comp}:{ModPartComponents[i].Quant}";
                    }
                    command = new NpgsqlCommand("UPDATE AssembledPart SET Components = ($1), Form = ($2), Notes = ($3) WHERE Part = ($4)", con)
                    {
                        Parameters =
                            {
                                new NpgsqlParameter { Value = c },
                                new NpgsqlParameter { Value = ModForm.Text },
                                new NpgsqlParameter { Value = tr.Text },
                                new NpgsqlParameter { Value = name }
                            }
                    };
                    command.ExecuteNonQuery();
                }
                con.Close();
                MessageBox.Show("Part Modified.");
                UpdateInventory();

            } catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
