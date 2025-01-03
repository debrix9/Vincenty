import customtkinter as ctk
from tkintermapview import TkinterMapView

# Set the theme for customtkinter
ctk.set_appearance_mode("dark")
ctk.set_default_color_theme("dark-blue")

root = ctk.CTk()
root.geometry("1200x800")
root.title("Vincenty Visualizer")

# Configure the root's grid layout
root.grid_columnconfigure(0, weight=3)  # Allocate more space to the left frame
root.grid_columnconfigure(1, weight=2)  # Right frame takes less space
root.grid_rowconfigure(0, weight=1)  # Allow rows to expand evenly

# Left frame for controls
left_frame = ctk.CTkFrame(root, corner_radius=10)
left_frame.grid(row=0, column=0, sticky="ns", padx=10, pady=10)
# Map widget on the right
right_frame = ctk.CTkFrame(root, corner_radius=10)
right_frame.grid(row=0, column=1, sticky="nsew", padx=10, pady=10)

# Map widget
map_widget = TkinterMapView(right_frame, width=580, height=380, corner_radius=10)
map_widget.pack(expand=True, fill="both", padx=10, pady=10)

# Adjust initial map position
map_widget.set_position(45.000, 9.000)
map_widget.set_zoom(10)

# Labels and placeholders for input
def update_labels():
    if input_mode_switch.get():  # Coordinates mode
        input_label_1.configure(text="Latitude")
        input_label_2.configure(text="Longitude")
        input_entry_1.configure(placeholder_text="Enter Latitude")
        input_entry_2.configure(placeholder_text="Enter Longitude")
    else:  # Bearing & Range mode
        input_label_1.configure(text="Bearing")
        input_label_2.configure(text="Range")
        input_entry_1.configure(placeholder_text="Enter Bearing")
        input_entry_2.configure(placeholder_text="Enter Range")


# Switch to toggle between "Coordinates" and "Bearing & Range"
toggle_mode_label = ctk.CTkLabel(left_frame, text="Input Mode", font=ctk.CTkFont(size=16))
toggle_mode_label.grid(row=0, column=0, columnspan=2, pady=10)

input_mode_switch = ctk.CTkSwitch(
    left_frame, text="Coordinates", command=update_labels
)
input_mode_switch.grid(row=1, column=0, columnspan=2, pady=10)

# Input fields and labels
input_label_1 = ctk.CTkLabel(left_frame, text="Latitude", font=ctk.CTkFont(size=14))
input_label_1.grid(row=2, column=0, pady=10, padx=10, sticky="w")

input_entry_1 = ctk.CTkEntry(left_frame, placeholder_text="Enter Latitude", width=200)  # Fixed width
input_entry_1.grid(row=2, column=1, pady=10, padx=10, sticky="e")

input_label_2 = ctk.CTkLabel(left_frame, text="Longitude", font=ctk.CTkFont(size=14))
input_label_2.grid(row=3, column=0, pady=10, padx=10, sticky="w")

input_entry_2 = ctk.CTkEntry(left_frame, placeholder_text="Enter Longitude", width=200)  # Fixed width
input_entry_2.grid(row=3, column=1, pady=10, padx=10, sticky="e")

# Call `update_labels` after defining labels and entries
update_labels()

# Button to process input and update map
def process_input():
    if input_mode_switch.get():  # Coordinates mode
        try:
            lat = float(input_entry_1.get())
            lon = float(input_entry_2.get())
            map_widget.set_position(lat, lon)
            print(f"Moved map to coordinates: ({lat}, {lon})")
        except ValueError:
            print("Invalid coordinates. Please enter valid numbers.")
    else:  # Bearing & Range mode
        try:
            bearing = float(input_entry_1.get())
            range_value = float(input_entry_2.get())
            print(f"Bearing: {bearing}, Range: {range_value}")
        except ValueError:
            print("Invalid bearing or range. Please enter valid numbers.")

generate_button = ctk.CTkButton(left_frame, text="Generate", command=process_input)
generate_button.grid(row=4, column=0, columnspan=2, pady=20)

# Run the application
root.mainloop()
