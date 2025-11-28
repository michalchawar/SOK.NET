/// <reference path="../lib/jquery/dist/jquery.js" />

// !!! TODO - add city support
const parseAddress = function (address = "") {


    return {
        street: "",
        buildingNumber: "",
        buildingLetter: "",
        apartmentNumber: "",
        apartmentLetter: "",
    }
}

const registerBuildingPicker = function (streetSelectElement, buildingSelectElement, buildingSelectOptions) {
    streetSelectElement.addEventListener("change", function () {
        const streetId = this.value;

        // Wyczyść poprzednie opcje budynków
        buildingSelectElement.innerHTML = '<option value="0" disabled selected>--Wybierz--</option>';
        if (streetId && streetId in buildingSelectOptions) {
            buildingSelectOptions[streetId].forEach(option => {
                const optionElement = document.createElement("option");

                optionElement.value = option.Value;
                optionElement.text = option.Text;
                optionElement.selected = option.Selected;
                optionElement.disabled = option.Disabled;

                buildingSelectElement.appendChild(optionElement);
            });
        }
    });
}