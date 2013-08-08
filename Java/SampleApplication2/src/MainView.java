/*
 * Copyright 2013 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

import java.awt.*;
import java.awt.event.*;
import javax.swing.*;
import javax.swing.table.*;
import javax.swing.event.*;

/**
 * The main view in the contact list application MVP model.
 * @author Alastair Wyse
 */
public class MainView extends JDialog implements IMainView {

    private IContactListPresenter presenter;
    
    // Components within the JDialog
    private Font defaultFont;
    private JTable grid;
    private GridTableModel gridModel;
    private JScrollPane gridScrollPane;
    private JTextField nameTextField;
    private JComboBox<String> categoryComboBox;
    private JTextField phoneTextField;
    private JTextField emailTextField;
    private JLabel nameLabel;
    private JLabel categoryLabel;
    private JLabel phoneLabel;
    private JLabel emailLabel;
    private JButton addUpdateButton;
    private JButton deleteButton;
    private JButton exitButton;
    
    /**
     * Initialises a new instance of the MainView class.
     */
    public MainView() throws Exception {
        SwingUtilities.invokeAndWait(new Runnable() {
            public void run() {
                createRootPane();
                dialogInit();
                InitializeVisibleProperties();
            }
        });
    }
    
    @Override
    public void SetPresenter(IContactListPresenter presenter) {
        this.presenter = presenter;
    }


    @Override
    public void Initialise() throws Exception {
        SwingUtilities.invokeAndWait(new Runnable() {
            public void run() {
                // Remove all rows from the grid
                while(gridModel.getRowCount() > 0) {
                    gridModel.removeRow(0);
                }
                // Blank out all fields
                nameTextField.setText("");
                categoryComboBox.removeAllItems();
                phoneTextField.setText("");
                emailTextField.setText("");
            }
        });
    }
    
    @Override
    public void Show() throws Exception {
        SwingUtilities.invokeAndWait(new Runnable() {
            public void run() {
                setVisible(true);
            }
        });
    }

    @Override
    public void Close() {
        SwingUtilities.invokeLater(new Runnable() {
            public void run() {
                setVisible(false);
            }
        });
    }

    @Override
    public void AddUpdateContactInGrid(String name, String category, String phoneNumber, String emailAddress) throws Exception {
        final String nameParam = name;
        final String categoryParam = category;
        final String phoneNumberParam = phoneNumber;
        final String emailAddressParam = emailAddress;
        SwingUtilities.invokeLater(new Runnable() {
            public void run() {
                int existingRow = -1;
                
                for(int i = 0; i < gridModel.getRowCount(); i = i + 1) {
                    if(nameParam.equals(gridModel.getValueAt(i, 0))) {
                        existingRow = i;
                    }
                }
                if(existingRow == -1) {
                    // A row for this contact does not exist, so insert
                    gridModel.addRow(new Object[] {nameParam, categoryParam, phoneNumberParam, emailAddressParam});
                }
                else {
                    // A row for this contact already exists, so update
                    gridModel.setValueAt(categoryParam, existingRow, 1);
                    gridModel.setValueAt(phoneNumberParam, existingRow, 2);
                    gridModel.setValueAt(emailAddressParam, existingRow, 3);
                }
            }
        });
    }

    @Override
    public void DeleteContactFromGrid(String name) throws Exception {
        final String nameParam = name;
        SwingUtilities.invokeLater(new Runnable() {
            public void run() {
                int rowId = -1;
                
                for(int i = 0; i < gridModel.getRowCount(); i = i + 1) {
                    if(nameParam.equals(gridModel.getValueAt(i, 0))) {
                        rowId = i;
                    }
                }
                if(rowId == -1) {
                    throw new IllegalArgumentException("A row for contact with name '" + nameParam + "' does not exist in the grid.");
                }
                else {
                    // Delete the row
                    gridModel.removeRow(rowId);
                }
            }
        });
    }

    @Override
    public void PopulateCategories(String[] categories) throws Exception {
        final String[] categoriesParam = categories;
        SwingUtilities.invokeAndWait(new Runnable() {
            public void run() {
                categoryComboBox.removeAllItems();
                for(int i = 0; i < categoriesParam.length; i = i + 1) {
                    categoryComboBox.addItem(categoriesParam[i]);
                }
            }
        });
    }

    @Override
    public void DisplayErrorMessage(String errorMessage) throws Exception {
        final String errorMessageParam = errorMessage;
        final MainView parent = this;

        SwingUtilities.invokeLater(new Runnable() {
            public void run() {
                JOptionPane.showMessageDialog(parent, errorMessageParam, "Error", JOptionPane.ERROR_MESSAGE);
            }
        }); 
    }

    /**
     * Sets all visible properties and components on the dialog
     */
    private void InitializeVisibleProperties() {

        // Setup basic properties
        setLayout(null);
        setSize(700, 295);
        setDefaultCloseOperation(JDialog.DO_NOTHING_ON_CLOSE);
        addWindowListener(new DefaultWindowAdapter());
        setLocationByPlatform(true);
        setResizable(false);
        setTitle("Contact List");
        
        defaultFont = new Font("Tahoma", Font.PLAIN, 11);

        // Setup the grid
        gridModel = new GridTableModel();
        gridModel.addColumn("Name");
        gridModel.addColumn("Category");
        gridModel.addColumn("Phone");
        gridModel.addColumn("Email");
        grid = new JTable(gridModel);
        grid.setFont(defaultFont);
        grid.setSelectionMode(ListSelectionModel.SINGLE_SELECTION);
        grid.getTableHeader().setFont(defaultFont);
        grid.getTableHeader().setReorderingAllowed(false);
        grid.getSelectionModel().addListSelectionListener(new GridRowSelectionHandler());
        gridScrollPane = new JScrollPane(grid);
        gridScrollPane.setBounds(13, 13, 485, 200);
        add(gridScrollPane);

        // Setup labels
        nameLabel = new JLabel();
        nameLabel.setText("Name");
        nameLabel.setSize(100, 13);
        nameLabel.setBounds(514, 12, 100, 13);
        nameLabel.setFont(defaultFont);
        add(nameLabel);
        
        categoryLabel = new JLabel();
        categoryLabel.setText("Category");
        categoryLabel.setSize(100, 13);
        categoryLabel.setBounds(514, 67, 100, 13);
        categoryLabel.setFont(defaultFont);
        add(categoryLabel);
        
        phoneLabel = new JLabel();
        phoneLabel.setText("Phone Number");
        phoneLabel.setSize(100, 13);
        phoneLabel.setBounds(514, 122, 100, 13);
        phoneLabel.setFont(defaultFont);
        add(phoneLabel);
        
        emailLabel = new JLabel();
        emailLabel.setText("Email Address");
        emailLabel.setSize(100, 13);
        emailLabel.setBounds(514, 177, 100, 13);
        emailLabel.setFont(defaultFont);
        add(emailLabel);

        // Setup fields
        nameTextField = new JTextField();
        nameTextField.setSize(160, 20);
        nameTextField.setBounds(518, 28, 160, 20);
        nameTextField.setFont(defaultFont);
        add(nameTextField);
        
        categoryComboBox = new JComboBox<String>();
        categoryComboBox.setSize(160, 20);
        categoryComboBox.setBounds(518, 83, 160, 20);
        categoryComboBox.setFont(defaultFont);
        add(categoryComboBox);
        
        phoneTextField = new JTextField();
        phoneTextField.setSize(160, 20);
        phoneTextField.setBounds(518, 138, 160, 20);
        phoneTextField.setFont(defaultFont);
        add(phoneTextField);
        
        emailTextField = new JTextField();
        emailTextField.setSize(160, 20);
        emailTextField.setBounds(518, 193, 160, 20);
        emailTextField.setFont(defaultFont);
        add(emailTextField);

        // Setup buttons
        addUpdateButton = new JButton();
        addUpdateButton.setText("Add/Update");
        addUpdateButton.setSize(95, 23);
        addUpdateButton.setBounds(514, 233, 95, 23);
        addUpdateButton.setFont(defaultFont);
        addUpdateButton.addActionListener(new AddUpdateButtonActionHandler());
        add(addUpdateButton);
        
        deleteButton = new JButton();
        deleteButton.setText("Delete");
        deleteButton.setSize(75, 23);
        deleteButton.setBounds(13, 233, 75, 23);
        deleteButton.setFont(defaultFont);
        deleteButton.addActionListener(new DeleteButtonActionHandler());
        add(deleteButton);
        
        exitButton = new JButton();
        exitButton.setText("Exit");
        exitButton.setSize(55, 23);
        exitButton.setBounds(624, 233, 55, 23);
        exitButton.setFont(defaultFont);
        exitButton.addActionListener(new ExitButtonActionHandler());
        add(exitButton);
    }
    
    /**
     * Handles when the add/update button is clicked.
     */
    private class AddUpdateButtonActionHandler implements ActionListener {
        @Override
        public void actionPerformed(ActionEvent a) {
            presenter.AddUpdateContact(nameTextField.getText(), (String)categoryComboBox.getSelectedItem(), phoneTextField.getText(), emailTextField.getText());
        }
    }
    
    /**
     * Handles when the delete button is clicked.
     */
    private class DeleteButtonActionHandler implements ActionListener {
        @Override
        public void actionPerformed(ActionEvent a) {
            if(grid.getSelectedRow() == -1) {
                JOptionPane.showMessageDialog(MainView.super, "Please select a contact in the grid.", "Select Contact", JOptionPane.WARNING_MESSAGE);
            }
            else {
                presenter.DeleteContact(nameTextField.getText());
            }
        }
    }
    
    /**
     * Handles when the exit button is clicked.
     */
    private class ExitButtonActionHandler implements ActionListener {
        @Override
        public void actionPerformed(ActionEvent a) {
            presenter.Exit();
        }
    }
    
    /**
     * Handles when the 'Close' button is clicked on the window.
     */
    private class DefaultWindowAdapter extends WindowAdapter {
        
        public DefaultWindowAdapter() {
            super();
        }
        
        @Override
        public void windowClosing(WindowEvent e) {
            presenter.Exit();
        }
    }
    
    /**
     * Handles when rows are selected within the grid.
     */
    private class GridRowSelectionHandler implements ListSelectionListener {

        @Override
        public void valueChanged(ListSelectionEvent e) {
            if(e.getValueIsAdjusting() == false) {
                int selectedRowIndex = grid.getSelectedRow();
                if(selectedRowIndex != -1) {
                    // If a row was selected, populate the fields with the contents of the selected grid row
                    nameTextField.setText((String)gridModel.getValueAt(selectedRowIndex, 0));
                    categoryComboBox.setSelectedItem((String)gridModel.getValueAt(selectedRowIndex, 1));
                    phoneTextField.setText((String)gridModel.getValueAt(selectedRowIndex, 2));
                    emailTextField.setText((String)gridModel.getValueAt(selectedRowIndex, 3));
                }
                else {
                    // Otherwise blank out all fields
                    nameTextField.setText("");
                    categoryComboBox.setSelectedItem("");
                    phoneTextField.setText("");
                    emailTextField.setText("");
                }
            }
        }
    }
    
    /**
     * Extension of the DefaultTableModel class, which defaults non-editable cells.
     */
    private class GridTableModel extends DefaultTableModel {
        
        public GridTableModel() {
            super();
        }
        
        @Override
        public boolean isCellEditable(int row,int cols) {
            return false;
        }
    }
}
