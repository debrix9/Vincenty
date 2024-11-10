import customtkinter as ctk
from tkintermapview import TkinterMapView

# Set the theme for customtkinter
ctk.set_appearance_mode("dark")
ctk.set_default_color_theme("dark-blue")

# Create the main window
root = ctk.CTk()
root.geometry("1200x800")
root.title("Vincenty visualizer")

# Left frame for sidebar
left_frame = ctk.CTkFrame(root, corner_radius=10)
left_frame.pack(side="left", fill="both", padx=10, pady=10, expand=True)

startingPosLabel = ctk.CTkLabel(left_frame, text="Starting Postion", font=ctk.CTkFont(size = 16))
startingPosLabel.pack(pady=10, padx=10)

# Text Box (not rounded for simplicity)
text_box = ctk.CTkTextbox(left_frame, width=160, height=80)
text_box.pack(pady=10, padx=10)

# Rounded Buttons
for i in range(3):
    button = ctk.CTkButton(left_frame, text=f"Button {i+1}", corner_radius=10, 
                            fg_color="gray", hover_color="darkgray")  # Set button color
    button.pack(pady=5, padx=1)
# Right frame for main content area with a map
right_frame = ctk.CTkFrame(root, corner_radius=10)
right_frame.pack(side="right", expand=True, fill="both", padx=10, pady=10)

# Add the map to the right frame
map_widget = TkinterMapView(right_frame, width=580, height=380, corner_radius=10)
map_widget.pack(expand=True, fill="both", padx=10, pady=10)

# Set the default location on the map
map_widget.set_position(45.000, 9.000)  
map_widget.set_zoom(10)  

# Run the application
root.mainloop()
