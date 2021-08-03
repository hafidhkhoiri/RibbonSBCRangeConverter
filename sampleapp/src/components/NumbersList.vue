<template>
<v-container fluid>
    <v-row>
        <v-col
        cols="12"
        md="12"
        >
            <v-dialog
                v-model="dialog"
                persistent
                max-width="600px"
                >
                <template v-slot:activator="{ on, attrs }">
                    
                    <v-btn
                    class="ma-2"
                        tile
                        color="primary"
                        v-bind="attrs"
                        v-on="on"
                        >
                        <v-icon left>
                            mdi-clipboard-text-search-outline
                        </v-icon>
                        Analyze
                    </v-btn>
                    <v-btn
                    class="ma-2  float-right"
                        tile
                        outlined
                        color="success"
                        v-bind="attrs"
                        v-on="on"
                        >
                        <v-icon left>
                            mdi-plus
                        </v-icon>
                        Add new numbers
                    </v-btn>
                </template>
                <v-card>
                    <v-card-title>
                    <span class="text-h5">Add new numbers</span>
                    </v-card-title>
                    <v-card-text>
                        
                        <v-form
                                ref="form"
                                v-model="valid"
                                lazy-validation
                            >
                            <v-container>
                                        <v-row>
                                            
                                        <v-col cols="12">
                                            <v-text-field
                                            label="Customer"
                                            value="Errai Pasifik"
                                            disabled
                                            ></v-text-field>
                                        </v-col>
                                        <v-col
                                            cols="12"
                                            sm="6"
                                            md="6"
                                        >
                                            <v-text-field
                                                required
                                                :rules="[rules.required, rules.counter, rules.onlyNumber]"
                                                label="Range Start"
                                                minLength="10"
                                                maxlength="12"
                                                counter="12"
                                            ></v-text-field>
                                        </v-col>
                                        <v-col
                                            cols="12"
                                            sm="6"
                                            md="6"
                                        >
                                            <v-text-field
                                                :rules="[rules.required, rules.counter, rules.onlyNumber]"
                                                required
                                                label="Range End"
                                                minLength="10"
                                                maxlength="12"
                                                counter="12"
                                                number
                                            ></v-text-field>
                                        </v-col>
                                        <v-col cols="12">
                                            <v-text-field
                                            label="Display Format"
                                            ></v-text-field>
                                        </v-col>
                                        <v-col
                                            cols="4"
                                        >
                                            <v-select
                                            :items="['UK', 'US']"
                                            label="Country"
                                            required
                                            ></v-select>
                                        </v-col>
                                        <v-col
                                            cols="8"
                                        >
                                            <v-select
                                            :items="['London(20)']"
                                            label="Area"
                                            required
                                            ></v-select>
                                        </v-col>

                                        <v-col
                                            cols="4"
                                        >
                                            <v-select
                                            :items="['Colt', 'Telkomsel']"
                                            label="Supplier"
                                            required
                                            ></v-select>
                                        </v-col>
                                        <v-col
                                            cols="8"
                                        >
                                            <v-select
                                            :items="['Colt Austria', 'Colt Africa']"
                                            label="Area"
                                            required
                                            ></v-select>
                                        </v-col>
                                        </v-row>
                                        <v-row>
                                            <v-col cols="12" style="padding-top:0;padding-bottom:0"><label>Add to following Ribbon SBC</label></v-col>
                                            <v-col cols="3">
                                                <v-checkbox
                                                label="AMER"
                                                ></v-checkbox>
                                            </v-col>
                                            <v-col cols="3">
                                                <v-checkbox
                                                label="EMEA"
                                                ></v-checkbox>
                                            </v-col>
                                            <v-col cols="3">
                                                <v-checkbox
                                                label="LATAM"
                                                ></v-checkbox>
                                            </v-col>
                                            <v-col cols="3">
                                                <v-checkbox
                                                label="APAC"
                                                ></v-checkbox>
                                            </v-col>
                                        </v-row>
                            </v-container>
                            
                        </v-form>

                    
                    <small>*indicates required field</small>
                    </v-card-text>
                    <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn
                        color="blue darken-1"
                        text
                        @click="dialog = false"
                    >
                        Close
                    </v-btn>
                    <v-btn
                        color="blue darken-1"
                        text
                        @click="validate"
                    >
                        Save
                    </v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            
            <v-card flat>
                <v-card-title>
                <v-text-field
                    v-model="search"
                    append-icon="mdi-magnify"
                    label="Search"
                    single-line
                    hide-details
                ></v-text-field>
                </v-card-title>
                
                <v-data-table  v-model="selectedItemsx"
                show-select
                dense
                :headers="fakeTable.headers"
                :items="fakes"
                :search="search"
                group-by="fakeTable.range"
                show-group-by
                class="elevation-1"
                >
                
                </v-data-table>
            </v-card>
        </v-col>
    </v-row>
</v-container>
    
</template>

<script>
export default {
    data: () => ({
            valid: false,
            dialog: false,
            rules: {
                required: value => !!value || 'Required.',
                counter: value => value.length <= 12 || 'Max 12 characters',
                onlyNumber: value => !isNaN(value) || 'Only numbers',
                email: value => {
                    const pattern = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
                    return pattern.test(value) || 'Invalid e-mail.'
                },
            },
            fakeTable:{
                search:'',
                selectedItemsx: [],
                headers: [
                    {
                        text: 'subRule_ID',
                        align: 'start',
                        sortable: false,
                        value: 'name',
                        groupable: false,
                    },
                    { text: 'DDI_digitMatch', value: 'calories', groupable: false },
                    { text: 'Number_of_digits', value: 'fat', groupable: false },
                    { text: 'Customer_SBC_Prefix', value: 'carbs', groupable: false },
                    { text: 'CriteriaName', value: 'protein', groupable: false },
                    { text: 'Note_1', value: 'iron', groupable: false },
                    { text: 'Range', value: 'range' },
                    ],
                    desserts: [
                    {
                        name: 'xxx',
                        calories: 159,
                        fat: 6.0,
                        carbs: 24,
                        protein: 4.0,
                        iron: '1%',
                        range: "1234500 to 123600"
                    },
                    {
                        name: 'xxx',
                        calories: 237,
                        fat: 9.0,
                        carbs: 37,
                        protein: 4.3,
                        iron: '1%',
                        range: "1234500 to 123600"
                    },
                    {
                        name: 'xxx',
                        calories: 262,
                        fat: 16.0,
                        carbs: 23,
                        protein: 6.0,
                        iron: '7%',
                        range: "1234500 to 123600"
                    },
                    {
                        name: 'xxx',
                        calories: 305,
                        fat: 3.7,
                        carbs: 67,
                        protein: 4.3,
                        iron: '8%',
                        range: "1234500 to 123600"
                    },
                    {
                        name: 'xxx',
                        calories: 356,
                        fat: 16.0,
                        carbs: 49,
                        protein: 3.9,
                        iron: '16%',
                        range: "1234500 to 123600"
                    },
                    {
                        name: 'xxx',
                        calories: 375,
                        fat: 0.0,
                        carbs: 94,
                        protein: 0.0,
                        iron: '0%',
                        range: "1234500 to 123600"
                    },
                    {
                        name: 'xxx',
                        calories: 392,
                        fat: 0.2,
                        carbs: 98,
                        protein: 0,
                        iron: '2%',
                        range: "4434500 to 443600"
                    },
                    {
                        name: 'xxx',
                        calories: 408,
                        fat: 3.2,
                        carbs: 87,
                        protein: 6.5,
                        iron: '45%',
                        range: "4434500 to 443600"
                    },
                    {
                        name: 'xxx',
                        calories: 452,
                        fat: 25.0,
                        carbs: 51,
                        protein: 4.9,
                        iron: '22%',
                        range: "4434500 to 443600"
                    },
                    {
                        name: 'xxx',
                        calories: 518,
                        fat: 26.0,
                        carbs: 65,
                        protein: 7,
                        iron: '6%',
                        range: "4434500 to 443600"
                    },
                    ],
                }
    }),
    computed:{
        fakes(){
            return [
                    {
                        id: 1,
                        name: 'xxx',
                        calories: 159,
                        fat: 6.0,
                        carbs: 24,
                        protein: 4.0,
                        iron: '1%',
                        range: "1234500 to 123600"
                    },
                    {
                        id: 2,
                        name: 'xxx',
                        calories: 237,
                        fat: 9.0,
                        carbs: 37,
                        protein: 4.3,
                        iron: '1%',
                        range: "1234500 to 123600"
                    },
                    {
                        id: 3,
                        name: 'xxx',
                        calories: 262,
                        fat: 16.0,
                        carbs: 23,
                        protein: 6.0,
                        iron: '7%',
                        range: "1234500 to 123600"
                    },
                    {
                        id: 4,
                        name: 'xxx',
                        calories: 305,
                        fat: 3.7,
                        carbs: 67,
                        protein: 4.3,
                        iron: '8%',
                        range: "1234500 to 123600"
                    },
                    {
                        id: 5,
                        name: 'xxx',
                        calories: 356,
                        fat: 16.0,
                        carbs: 49,
                        protein: 3.9,
                        iron: '16%',
                        range: "1234500 to 123600"
                    },
                    {
                        id: 6,
                        name: 'xxx',
                        calories: 375,
                        fat: 0.0,
                        carbs: 94,
                        protein: 0.0,
                        iron: '0%',
                        range: "1234500 to 123600"
                    },
                    {
                        id: 7,
                        name: 'xxx',
                        calories: 392,
                        fat: 0.2,
                        carbs: 98,
                        protein: 0,
                        iron: '2%',
                        range: "4434500 to 443600"
                    },
                    {
                        id: 8,
                        name: 'xxx',
                        calories: 408,
                        fat: 3.2,
                        carbs: 87,
                        protein: 6.5,
                        iron: '45%',
                        range: "4434500 to 443600"
                    },
                    {
                        id: 9,
                        name: 'xxx',
                        calories: 452,
                        fat: 25.0,
                        carbs: 51,
                        protein: 4.9,
                        iron: '22%',
                        range: "4434500 to 443600"
                    },
                    {
                        id: 10,
                        name: 'xxx',
                        calories: 518,
                        fat: 26.0,
                        carbs: 65,
                        protein: 7,
                        iron: '6%',
                        range: "4434500 to 443600"
                    },
                    ]
        }
    },
    methods: {
      validate () {
        this.$refs.form.validate()
      },
      reset () {
        this.$refs.form.reset()
      },
      resetValidation () {
        this.$refs.form.resetValidation()
      },
    },
}
</script>
