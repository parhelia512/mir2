﻿using Server.MirEnvir;

namespace Server
{
    public partial class GameShop : Form
    {

        private List<GameShopItem> SelectedItems;

        public Envir Envir => SMain.EditEnvir;

        public GameShop()
        {
            InitializeComponent();

            LoadGameShopItems();

            GameShopSearchBox_TextChanged(this, EventArgs.Empty);
        }

        private void GameShop_Load(object sender, EventArgs e)
        {
            UpdateInterface();
            LoadItemsIntoComboBox();
            ItemComboBox.SelectedIndex = -1;
        }

        private void GameShop_FormClosed(object sender, FormClosedEventArgs e)
        {
            Envir.SaveDB();
        }

        public class ListBoxItem
        {
            public string DisplayMember { get; set; }
            public object ValueMember { get; set; }

            public override string ToString()
            {
                return DisplayMember;
            }
        }

        private void LoadGameShopItems()
        {


            ClassFilter_lb.Items.Clear();
            CategoryFilter_lb.Items.Clear();
            GameShopListBox.Items.Clear();

            ClassFilter_lb.Items.Add("All Classes");
            CategoryFilter_lb.Items.Add("All Categories");

            for (int i = 0; i < SMain.EditEnvir.GameShopList.Count; i++)
            {
                if (!ClassFilter_lb.Items.Contains(SMain.EditEnvir.GameShopList[i].Class)) ClassFilter_lb.Items.Add(SMain.EditEnvir.GameShopList[i].Class);
                if (!CategoryFilter_lb.Items.Contains(SMain.EditEnvir.GameShopList[i].Category)) CategoryFilter_lb.Items.Add(SMain.EditEnvir.GameShopList[i].Category);

                GameShopListBox.Items.Add(SMain.EditEnvir.GameShopList[i]);
            }

            ClassFilter_lb.Text = "All Classes";
            CategoryFilter_lb.Text = "All Categories";
            SectionFilter_lb.Text = "All Items";
        }

        private void UpdateGameShopList()
        {

            GameShopListBox.Items.Clear();
            for (int i = 0; i < SMain.EditEnvir.GameShopList.Count; i++)
            {
                if (ClassFilter_lb.Text == "All Classes" || SMain.EditEnvir.GameShopList[i].Class == ClassFilter_lb.Text)
                    if (SectionFilter_lb.Text == "All Items" || SMain.EditEnvir.GameShopList[i].TopItem && SectionFilter_lb.Text == "Top Items" || SMain.EditEnvir.GameShopList[i].Deal && SectionFilter_lb.Text == "Sale Items" || SMain.EditEnvir.GameShopList[i].Date > Envir.Now.AddDays(-7) && SectionFilter_lb.Text == "New Items")
                        if (CategoryFilter_lb.Text == "All Categories" || SMain.EditEnvir.GameShopList[i].Category == CategoryFilter_lb.Text)
                            GameShopListBox.Items.Add(SMain.EditEnvir.GameShopList[i]);
            }
        }

        private void GameShopListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInterface();
        }

        public void UpdateInterface(bool refreshList = false)
        {
            SelectedItems = GameShopListBox.SelectedItems.Cast<GameShopItem>().ToList();

            if (SelectedItems.Count == 0)
            {
                GoldPrice_textbox.Text = String.Empty;
                GPPrice_textbox.Text = String.Empty;
                Stock_textbox.Text = String.Empty;
                Individual_checkbox.Checked = false;
                Class_combo.Text = "All";
                Category_textbox.Text = "";
                TopItem_checkbox.Checked = false;
                DealofDay_checkbox.Checked = false;
                CredxGold_textbox.Text = Settings.CredxGold.ToString();
                ItemDetails_gb.Visible = false;
                TotalSold_label.Text = "0";
                LeftinStock_label.Text = "";
                Count_textbox.Text = String.Empty;
                CreditOnlyBox.Checked = false;
                GoldOnlyBox.Checked = false;

                // Reset ComboBox
                ItemComboBox.SelectedIndex = -1;

                return;
            }

            ItemDetails_gb.Visible = true;

            GoldPrice_textbox.Text = SelectedItems[0].GoldPrice.ToString();
            GPPrice_textbox.Text = SelectedItems[0].CreditPrice.ToString();
            Stock_textbox.Text = SelectedItems[0].Stock.ToString();
            Individual_checkbox.Checked = SelectedItems[0].iStock;
            Class_combo.Text = SelectedItems[0].Class;
            Category_textbox.Text = SelectedItems[0].Category;
            TopItem_checkbox.Checked = SelectedItems[0].TopItem;
            DealofDay_checkbox.Checked = SelectedItems[0].Deal;
            Count_textbox.Text = SelectedItems[0].Count.ToString();
            CreditOnlyBox.Checked = SelectedItems[0].CanBuyCredit;
            GoldOnlyBox.Checked = SelectedItems[0].CanBuyGold;

            // Set the ItemComboBox selection to match the ItemIndex
            if (SelectedItems[0].Info != null && !string.IsNullOrEmpty(SelectedItems[0].Info.Name))
            {
                var itemName = SelectedItems[0].Info.Name;

                // Select the corresponding item in the ComboBox
                if (ItemComboBox.Items.Contains(itemName))
                {
                    ItemComboBox.SelectedItem = itemName;
                }
                else
                {
                    ItemComboBox.SelectedIndex = -1; // Reset if no match found
                }
            }
            else
            {
                ItemComboBox.SelectedIndex = -1; // Reset if no valid Info or Name
            }

            GetStats();
        }

        private void GetStats()
        {
            int purchased;

            SMain.Envir.GameshopLog.TryGetValue(SelectedItems[0].GIndex, out purchased);
            TotalSold_label.Text = purchased.ToString();

            if (!Individual_checkbox.Checked && SelectedItems[0].Stock != 0)
            {
                if (SelectedItems[0].Stock - purchased >= 0)
                    LeftinStock_label.Text = (SelectedItems[0].Stock - purchased).ToString();
                else
                    LeftinStock_label.Text = "";
            }
            else if (SelectedItems[0].Stock == 0)
            {
                LeftinStock_label.Text = "Infinite";
            }
            else if (Individual_checkbox.Checked)
            {
                LeftinStock_label.Text = "Can't calc individual levels";
            }
        }

        private void GoldPrice_textbox_TextChanged(object sender, EventArgs e)
        {

            uint temp;

            if (!uint.TryParse(GoldPrice_textbox.Text, out temp))
            {
                GoldPrice_textbox.BackColor = Color.Red;
                return;
            }

            GoldPrice_textbox.BackColor = SystemColors.Window;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].GoldPrice = temp;
        }

        private void GPPrice_textbox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            uint temp;

            if (!uint.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }

            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].CreditPrice = temp;

            if (ActiveControl.Text != "") GoldPrice_textbox.Text = (temp * Settings.CredxGold).ToString();
        }

        private void Class_combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;
            string temp = ActiveControl.Text;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].Class = temp;
        }

        private void TopItem_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].TopItem = TopItem_checkbox.Checked;
        }

        private void Remove_button_Click(object sender, EventArgs e)
        {
            if (SelectedItems.Count == 0) return;

            if (MessageBox.Show("Are you sure you want to remove the selected Items?", "Remove Items?", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            for (int i = 0; i < SelectedItems.Count; i++) Envir.Remove(SelectedItems[i]);

            LoadGameShopItems();
            UpdateInterface();
        }

        private void DealofDay_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].Deal = DealofDay_checkbox.Checked;
        }

        private void Category_textbox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;
            string temp = ActiveControl.Text;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].Category = temp;
        }

        private void Stock_textbox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            int temp;

            if (!int.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }

            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].Stock = temp;

            GetStats();
        }

        private void Individual_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].iStock = Individual_checkbox.Checked;

        }

        private void CredxGold_textbox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            short temp;

            if (!short.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }

            ActiveControl.BackColor = SystemColors.Window;
            Settings.CredxGold = temp;
        }

        private void Count_textbox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp > 999)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }

            if (temp < 1)
            {
                temp = 1;
                ActiveControl.Text = "1";
            }
            else if (temp > SelectedItems[0].Info.StackSize)
            {
                temp = SelectedItems[0].Info.StackSize;
                ActiveControl.Text = SelectedItems[0].Info.StackSize.ToString();
            }

            ActiveControl.BackColor = SystemColors.Window;
            SelectedItems[0].Count = temp;
        }

        private void ClassFilter_lb_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGameShopList();
        }

        private void SectionFilter_lb_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGameShopList();
        }

        private void CategoryFilter_lb_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGameShopList();
        }

        private void ResetFilter_button_Click(object sender, EventArgs e)
        {
            ClassFilter_lb.Text = "All Classes";
            CategoryFilter_lb.Text = "All Categories";
            SectionFilter_lb.Text = "All Items";
            UpdateGameShopList();

        }

        private void ServerLog_button_Click(object sender, EventArgs e)
        {
            if (SMain.Envir.Running)
            {
                if (MessageBox.Show("Reseting purchase logs cannot be reverted and will set stock levels back to defaults, This will take effect instantly.", "Remove Logs?", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                SMain.Envir.ClearGameshopLog();
            }
            else
            {
                if (MessageBox.Show("Reseting purchase logs cannot be reverted and will set stock levels back to defaults, This will take effect when you start the server", "Remove Logs?", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                SMain.Envir.ResetGS = true;
            }
        }
        private void GoldOnlyBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender)
                return;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].CanBuyGold = GoldOnlyBox.Checked;
        }

        private void CreditOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender)
                return;

            for (int i = 0; i < SelectedItems.Count; i++)
                SelectedItems[i].CanBuyCredit = CreditOnlyBox.Checked;
        }

        #region Load Items
        private void LoadItemsIntoComboBox()
        {
            ItemComboBox.Items.Clear();

            // Add "None" as a default option
            ItemComboBox.Items.Add("None");

            // Add all items from ItemInfoList
            foreach (var item in SMain.EditEnvir.ItemInfoList)
            {
                if (!string.IsNullOrEmpty(item.Name))
                {
                    ItemComboBox.Items.Add($"{item.Name}");
                }
            }

            ItemComboBox.SelectedIndex = 0; // Default to "None"
        }
        #endregion

        private void Add_Button_Click(object sender, EventArgs e)
        {
            if (SMain.EditEnvir.ItemInfoList == null || SMain.EditEnvir.ItemInfoList.Count == 0)
            {
                MessageBox.Show("No items available to add.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the first item's index as default
            var defaultItem = SMain.EditEnvir.ItemInfoList.First();
            int firstItemIndex = defaultItem.Index;

            // Find the next available GIndex
            int nextGIndex = SMain.EditEnvir.GameShopList.Count > 0
                ? SMain.EditEnvir.GameShopList.Max(item => item.GIndex) + 1
                : 1;

            // Create the new GameShopItem
            var newItem = new GameShopItem
            {
                GIndex = nextGIndex,
                GoldPrice = 0,
                CreditPrice = 0,
                ItemIndex = firstItemIndex,
                Info = defaultItem,
                Date = DateTime.Now,
                Class = "None",
                Category = "None"
            };

            // Add to GameShopList (main data source)
            SMain.EditEnvir.GameShopList.Add(newItem);

            // Add to GameShopListBox for UI display
            GameShopListBox.Items.Add(newItem);

            // Set ComboBox to the first item's name
            ItemComboBox.SelectedItem = $"{defaultItem.Name}";

            // Save the database
            Envir.SaveDB();
        }

        private void ItemComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ensure we have a selected GameShopItem
            if (SelectedItems == null || SelectedItems.Count == 0)
                return;

            // Get the selected item name
            var selectedName = ItemComboBox.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedName) || selectedName == "None")
                return;

            // Find the corresponding ItemInfo object by name
            var newItemInfo = SMain.EditEnvir.ItemInfoList
                .FirstOrDefault(x => x.Name == selectedName);

            if (newItemInfo == null)
                return;

            // Update the selected GameShopItem
            var selectedGameShopItem = SelectedItems[0];
            selectedGameShopItem.ItemIndex = newItemInfo.Index;
            selectedGameShopItem.Info = newItemInfo;

            // Refresh the GameShopListBox to reflect changes
            int selectedIndex = GameShopListBox.SelectedIndex;
            GameShopListBox.Items[selectedIndex] = selectedGameShopItem;
        }

        #region Search Box
        private void GameShopSearchBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = GameShopSearchBox.Text.Trim().ToLower();

            GameShopListBox.Items.Clear();

            foreach (var item in SMain.EditEnvir.GameShopList)
            {
                // Add to list if search text is empty or the item matches the search criteria
                if (string.IsNullOrEmpty(searchText) ||
                    (!string.IsNullOrEmpty(item.Info?.Name) && item.Info.Name.ToLower().Contains(searchText)))
                {
                    GameShopListBox.Items.Add(item);
                }
            }
        }

        #endregion

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (GameShopListBox.Items.Count == 0)
            {
                MessageBox.Show("No items to export.", "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string exportDir = Path.Combine(Application.StartupPath, "Exports");
            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Title = "GameShop Info",
                Filter = "Text Files (*.txt)|*.txt",
                FileName = "GameShop_Export.txt",
                InitialDirectory = exportDir
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            List<string> lines = new List<string>
            {
                "GameShop Info",
                ""
            };

            foreach (var obj in GameShopListBox.Items)
            {
                if (obj is GameShopItem item && item.Info != null)
                {
                    string name = item.Info.Name;
                    uint gp = item.CreditPrice;
                    uint gold = item.GoldPrice;

                    lines.Add($"{name}: GameGold: {gp:n0} Gold: {gold:n0}");
                }
            }

            File.WriteAllLines(saveDialog.FileName, lines);

            MessageBox.Show("Export complete.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
