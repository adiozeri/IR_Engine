   M S B A M L   `   `   `  ����D  @IR_engine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:IR_engine	IR_engine  5   	   !    IR_engine.SaveQueriesToFile   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:IR_engine     5   	     � Title$  SaveQueriesToFile��5   	   $	��300��6#   $��511.333��60   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $��1* �6   �� 5      $��3* �6   �� 5      $��1* �6    � RowDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   .+�5   
   �� $��
File name:��6   $��27,83,177,30.333q�6&   $��2��6@   �� 5   
   -   5         ��Name$ fileNameTextBox��5      $��23��5      $
��Left=�5      $��29.667,83,0,0q�5      $	��Top=�5      $	��236��5      $��1��6   �� 5   
   $��File path path:��6   $��27,23.667,172,87.667q�6+   $��2��6I   $��1��6]   �� 5   
   -   5        $ FilePathTextBox��5"       �� 
IsReadOnly . $��23��5      $
��Left=�5       $��29.667,23.667,0,0q�5!      $	��Top=�5#      $	��236��5$      $��1��6   $��1��6)   �� 5%   
   -   5         ��Name$ saveFile��5(      $��25��5%      $
��Left=�5&      $��116.667,76.667,0,0q�5'      $��1q�5)      $	��Top=�6   $��45��5*      $��1��61    J� RenderTransformOrigin$ -3.067,3.053&�6A   $��1��6f   .��5+      �� $��10(�6   $��Normal"�5,      ���	#FF3C3C3C$
��Save��5.      $
��Wrap=�5/      $ -3.646,5.525&�6%   50      �� 51   
   $��10,10,15.333,84.333q�6   $��3��61   �� 6F   $
��Bold"�6J   $��18(�6\   $$�Choose File Name And File Path��6j   6�   �� 52   
   -   5        $ BrwoserPath��55      $��25��52      $
��Left=�53      $��0,23.667,0,0q�54      $��1q�56      $	��Top=�6   $��45��57      $��2��62   $ -3.067,3.053&�6B   $��1��6g   .��58      �� $��10(�6   $��Normal"�59      ���	#FF3C3C3C$��Browse��5;      $
��Wrap=�5<      $ -3.646,5.525&�6%   5=      5>      5?      